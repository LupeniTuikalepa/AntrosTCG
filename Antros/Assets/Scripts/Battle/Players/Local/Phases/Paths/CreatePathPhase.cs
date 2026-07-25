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
        private Dictionary<HexCoordinates, MovementStep> currentCameFrom;

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

                Dictionary<HexCoordinates, int> costSoFar = DictionaryPool<HexCoordinates, int>.Get();
                Dictionary<HexCoordinates, MovementStep> cameFrom = DictionaryPool<HexCoordinates, MovementStep>.Get();
                try
                {
                    MovementReachability.GetReachable(agent, BattleGrid, center, remaining, costSoFar, cameFrom);

                    // Nowhere to go from here.
                    if (cameFrom.Count == 0)
                        break;

                    BuildRings(center, costSoFar);
                    currentCenter = center;
                    currentCameFrom = cameFrom;

                    using HexPatternBuilder builder = BuildPattern(center, costSoFar);

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
                    if (!costSoFar.TryGetValue(goal, out int goalCost) || goalCost <= 0)
                        continue;

                    MovementReachability.TryBuildPath(center, goal, cameFrom, CurrentPath);
                    remaining -= goalCost;
                    center = goal;

                    OnPathChanged?.Invoke(this);
                }
                finally
                {
                    currentCameFrom = null;
                    DictionaryPool<HexCoordinates, int>.Release(costSoFar);
                    DictionaryPool<HexCoordinates, MovementStep>.Release(cameFrom);
                }
            }

            directRing.Clear();
            reachableRing.Clear();
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
            if (reachableRing.Contains(coord))
                return HighlightState.Preview2;

            return fallback;
        }

        private void BuildRings(HexCoordinates center, Dictionary<HexCoordinates, int> costSoFar)
        {
            directRing.Clear();
            reachableRing.Clear();

            foreach (KeyValuePair<HexCoordinates, int> kv in costSoFar)
            {
                if (kv.Key == center)
                    continue;

                if (kv.Value == 1)
                    directRing.Add(kv.Key);
                else
                    reachableRing.Add(kv.Key);
            }
        }

        private HexPatternBuilder BuildPattern(HexCoordinates center, Dictionary<HexCoordinates, int> costSoFar)
        {
            using (ListPool<HexCoordinates>.Get(out var coords))
            {
                foreach (KeyValuePair<HexCoordinates, int> kv in costSoFar)
                    if (kv.Key != center)
                        coords.Add(kv.Key);

                // The builder copies the coordinates into its own set; the controller is only
                // used when building from patterns, which we don't do here.
                return new HexPatternBuilder(coords, new BattleIgnoreOriginPatternController(BattleGrid, center));
            }
        }

        private void UpdateTemporaryPath(EntityAddress address)
        {
            if (currentCameFrom == null)
                return;
            if (!address.TryGetComponentRO(out GridMemberComponent gridMember))
                return;

            TemporaryPath.Clear();
            MovementReachability.TryBuildPath(currentCenter, gridMember.coordinates, currentCameFrom, TemporaryPath);
            OnPathChanged?.Invoke(this);
        }

        private void ClearTemporaryPath(EntityAddress address)
        {
            TemporaryPath.Clear();
            OnPathChanged?.Invoke(this);
        }
    }
}
