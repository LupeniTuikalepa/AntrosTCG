using System;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;
using ATCG.Metrics;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface ICreatePathPhase : ILocalPlayerPhase
    {
        event Action<ICreatePathPhase> OnPathChanged;

        HexCoordinates StartingPoint { get; }
        List<HexCoordinates> CurrentPath { get; }
        List<HexCoordinates> TemporaryPath { get; }

        // Highlight categories for the current step (both are selectable):
        //  - DirectRing:    the imposed ring-1 tiles (cost 1) for step-by-step building.
        //  - ReachableRing: tiles reachable within the remaining speed (cost > 1), for
        //                   "fast travel" — clicking one appends the whole computed path.
        IReadOnlyCollection<HexCoordinates> DirectRing { get; }
        IReadOnlyCollection<HexCoordinates> ReachableRing { get; }
    }

    /// <summary>
    /// Builds a movement path tile-by-tile. The game imposes a ring-1 pattern around the unit
    /// and the speed is the number of tiles it can cross. Each step the player either clicks an
    /// adjacent tile (1 speed) or a farther reachable tile (fast travel: pathfinding fills the
    /// gap, consuming the corresponding speed). Redirect slides are free and folded into the
    /// reachability flood.
    ///
    /// Each step computes ONE <see cref="ReachableMap"/> up front (redirects included) and keeps
    /// it for the whole step. Rings, hover preview and the committed path all read that map, so
    /// what the editor previews is exactly what the move will do — no per-hover recompute.
    /// </summary>
    public class CreatePathPhase : LocalPlayerPhase<HexCoordinates[]>, ICreatePathPhase, IHighlightingPhase
    {
        private readonly struct GridFilter : IEntityFilter
        {
            public bool Accepts(EntityAddress entityAddress)
            {
                return entityAddress.Is<BattleCellAspect>(out var cell) && cell.CanBeMovedOn();
            }
        }

        public event Action<ICreatePathPhase> OnPathChanged;

        public HexCoordinates StartingPoint { get; }
        public int Speed { get; }

        public List<HexCoordinates> CurrentPath { get; private set; }
        public List<HexCoordinates> TemporaryPath { get; private set; }

        private readonly HashSet<HexCoordinates> directRing = new();
        private readonly HashSet<HexCoordinates> reachableRing = new();
        // Redirect cells: selectable, but they have no destination of their own — aiming at one
        // resolves to the landing it pushes you to (see ReachableMap.TryResolveTarget).
        private readonly HashSet<HexCoordinates> redirectRing = new();

        public IReadOnlyCollection<HexCoordinates> DirectRing => directRing;
        public IReadOnlyCollection<HexCoordinates> ReachableRing => reachableRing;

        // IHighlightingPhase: contributes the movement colour theme while active.
        public ChannelKey HighlightChannel { get; private set; }
        public HighlightTheme HighlightTheme => GameMetrics.Current.HighlightSettings != null
            ? GameMetrics.Current.HighlightSettings.MovementTheme
            : null;

        // The moving entity — carries a PathfindingAgentComponent, resolved to an aspect here.
        private readonly EntityAddress agentAddress;

        private PathfindingAgentAspect agent;

        // Set for the duration of a step so the hover handler can preview the tentative path.
        private HexCoordinates currentCenter;
        private ReachableMap currentReachable;

        public CreatePathPhase(LocalBattlePlayer localBattlePlayer, EntityAddress agentAddress, HexCoordinates startingPoint, int speed) : base(localBattlePlayer)
        {
            this.agentAddress = agentAddress;
            this.StartingPoint = startingPoint;
            this.Speed = speed;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            CurrentPath = ListPool<HexCoordinates>.Get();
            TemporaryPath = ListPool<HexCoordinates>.Get();
            HighlightChannel = ChannelKey.GetUniqueChannelKey();
            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            ListPool<HexCoordinates>.Release(CurrentPath);
            ListPool<HexCoordinates>.Release(TemporaryPath);
            directRing.Clear();
            reachableRing.Clear();
            redirectRing.Clear();
            return base.Dispose(token);
        }

        protected override async Awaitable<HexCoordinates[]> Execute(CancellationToken token)
        {
            if (!agentAddress.Is(out agent))
            {
                Debug.LogWarning($"[CreatePathPhase] Entity {agentAddress.entity} has no PathfindingAgentAspect " +
                                 "(missing PathfindingAgentComponent?) — movement aborted.");
                return Array.Empty<HexCoordinates>();
            }

            var filter = new GridFilter();
            HexCoordinates center = StartingPoint;
            int remaining = Speed;

            CurrentPath.Add(StartingPoint);

            while (remaining > 0)
            {
                if (!BattleGrid.TryGetBattleCell(center, out _))
                    break;

                ReachableMap map = HexPathfinder.ComputeReachable(agent, BattleGrid, center, remaining);
                try
                {
                    // Nowhere to go from here.
                    if (!map.HasReachableTiles)
                        break;

                    BuildRings(center, map);
                    currentCenter = center;
                    currentReachable = map;

                    using HexPatternBuilder builder = BuildPattern(center, map);

                    var selectEntityPhase = new SelectEntityPhase<GridFilter>(LocalBattlePlayer, filter, builder);
                    selectEntityPhase.SetHighlightClassifier(ClassifyMovement);
                    selectEntityPhase.OnEntityHovered += UpdateTemporaryPath;
                    selectEntityPhase.OnEntityUnhovered += ClearTemporaryPath;

                    EntityAddress[] result = await selectEntityPhase;

                    selectEntityPhase.OnEntityHovered -= UpdateTemporaryPath;
                    selectEntityPhase.OnEntityUnhovered -= ClearTemporaryPath;
                    TemporaryPath.Clear();

                    // Empty selection = cancel the whole move.
                    if (result.Length <= 0)
                        return Array.Empty<HexCoordinates>();

                    if (!result[0].TryGetComponentRO(out GridMemberComponent gridMember))
                        continue;

                    HexCoordinates goal = gridMember.coordinates;
                    // A redirect cell resolves to the landing it pushes to; a normal reachable tile
                    // resolves to itself. Either way `target` is where the unit actually ends up.
                    if (!map.TryResolveTarget(goal, out HexCoordinates target, out int goalCost) || goalCost <= 0)
                        continue;

                    AppendStepPath(map, target);
                    remaining -= goalCost;
                    center = target;

                    OnPathChanged?.Invoke(this);
                }
                finally
                {
                    currentReachable = null;
                    map.Dispose();
                }
            }

            directRing.Clear();
            reachableRing.Clear();
            redirectRing.Clear();
            return CurrentPath.ToArray();
        }

        // Refines the select phase's base state: direct ring -> Preview1, reachable -> Preview2.
        private HighlightState ClassifyMovement(EntityAddress address, HighlightState fallback)
        {
            if (!address.TryGetComponentRO(out GridMemberComponent gridMember))
                return fallback;

            HexCoordinates coord = gridMember.coordinates;
            if (directRing.Contains(coord))
                return HighlightState.Preview1;
            // Redirect cells get their own slot (Preview6) so they read as distinct from plain
            // reachable / fast-travel tiles.
            if (redirectRing.Contains(coord))
                return HighlightState.Preview6;
            if (reachableRing.Contains(coord))
                return HighlightState.Preview2;

            return fallback;
        }

        private void BuildRings(HexCoordinates center, ReachableMap map)
        {
            directRing.Clear();
            reachableRing.Clear();
            redirectRing.Clear();

            foreach (KeyValuePair<HexCoordinates, int> kv in map.Costs)
            {
                if (kv.Key == center)
                    continue;

                if (kv.Value == 1)
                    directRing.Add(kv.Key);
                else
                    reachableRing.Add(kv.Key);
            }

            // Redirect cells are selectable proxies for their landing (pruned so none overlaps a
            // real reachable tile).
            foreach (KeyValuePair<HexCoordinates, HexCoordinates> kv in map.RedirectTargets)
                redirectRing.Add(kv.Key);
        }

        private HexPatternBuilder BuildPattern(HexCoordinates center, ReachableMap map)
        {
            using (ListPool<HexCoordinates>.Get(out var coords))
            {
                foreach (KeyValuePair<HexCoordinates, int> kv in map.Costs)
                    if (kv.Key != center)
                        coords.Add(kv.Key);

                // Redirect cells join the selectable pattern too, so the player can aim at one.
                foreach (KeyValuePair<HexCoordinates, HexCoordinates> kv in map.RedirectTargets)
                    coords.Add(kv.Key);

                // The builder copies the coordinates into its own set; the controller is only
                // used when building from patterns, which we don't do here.
                return new HexPatternBuilder(coords, new BattleIgnoreOriginPatternController(BattleGrid, center));
            }
        }

        // Appends this step's committed segment (center-exclusive .. goal, redirect slides
        // included) to CurrentPath, which already ends at `center`.
        private void AppendStepPath(ReachableMap map, HexCoordinates goal)
        {
            using (ListPool<HexCoordinates>.Get(out var segment))
            {
                if (!map.TryGetPathFor(goal, segment))
                    return;

                for (int i = 1; i < segment.Count; i++)
                    CurrentPath.Add(segment[i]);
            }
        }

        private void UpdateTemporaryPath(EntityAddress address)
        {
            if (currentReachable == null)
                return;
            if (!address.TryGetComponentRO(out GridMemberComponent gridMember))
                return;

            // Aiming at a redirect cell previews the LANDING's path — as if the cursor were on that
            // landing (the redirect cell has no preview of its own). Full path is center-inclusive;
            // drop the leading center since CurrentPath already ends there and the renderer draws
            // CurrentPath + TemporaryPath back to back.
            TemporaryPath.Clear();
            if (currentReachable.TryResolveTarget(gridMember.coordinates, out HexCoordinates target, out _)
                && currentReachable.TryGetPathFor(target, TemporaryPath)
                && TemporaryPath.Count > 0)
                TemporaryPath.RemoveAt(0);

            OnPathChanged?.Invoke(this);
        }

        private void ClearTemporaryPath(EntityAddress address)
        {
            TemporaryPath.Clear();
            OnPathChanged?.Invoke(this);
        }
    }
}
