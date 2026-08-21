using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids
{
    /// <summary>
    /// A single accepted move: where it started from and every tile physically traversed
    /// (the chosen neighbour, plus any tiles a redirect slid the unit across, in forward
    /// order). Used to rebuild the full path including redirect slides.
    /// </summary>
    public readonly struct MovementStep
    {
        public readonly HexCoordinates previous;
        public readonly HexCoordinates[] traversed;

        public MovementStep(HexCoordinates previous, HexCoordinates[] traversed)
        {
            this.previous = previous;
            this.traversed = traversed;
        }
    }

    /// <summary>
    /// Tile-by-tile movement planning on the hex grid for a <see cref="PathfindingAgentAspect"/>.
    ///
    /// Model: the game imposes a ring-1 pattern around the unit. Each chosen neighbour costs
    /// exactly 1 step (= 1 speed). A redirect status on the tile you step ONTO slides you
    /// further in your entry direction (previous -> chosen), recursively, for FREE — the slide
    /// does not cost extra speed.
    ///
    /// There is exactly ONE reachability computation, <see cref="ComputeReachable"/>, which folds
    /// redirects in (through <see cref="ResolveRedirect"/>, the single place redirects are resolved)
    /// and returns a <see cref="ReachableMap"/>. Highlight rings, hover preview, path commit and AI
    /// all read that map, so "reachable" and "the path shown/taken" can never disagree.
    /// </summary>
    public static class HexPathfinder
    {
        /// <summary>
        /// Floods every tile reachable within <paramref name="maxSteps"/> steps from
        /// <paramref name="origin"/>, folding redirect slides in, and returns a
        /// <see cref="ReachableMap"/> with per-tile cost and precomputed full paths. This is the
        /// ONLY entry point for reachability — dispose the returned map when done (it borrows
        /// pooled collections).
        /// </summary>
        public static ReachableMap ComputeReachable(
            PathfindingAgentAspect agent, BattleGrid grid, HexCoordinates origin, int maxSteps)
        {
            var map = new ReachableMap(origin);

            if (maxSteps > 0)
                Flood(agent, grid, origin, maxSteps, map);

            map.BakePaths();
            return map;
        }

        /// <summary>
        /// Convenience wrapper for callers that only want a single path (e.g. AI). Reaches
        /// <paramref name="goal"/> if possible, otherwise the reachable tile closest to it
        /// (see <see cref="ReachableMap.TryGetPathToward"/>). Writes the full origin-inclusive
        /// path into <paramref name="path"/> (cleared first). Shares the exact redirect-aware
        /// computation used everywhere else.
        /// </summary>
        public static bool TryBuildPath(
            HexCoordinates origin, HexCoordinates goal,
            PathfindingAgentAspect agent, List<HexCoordinates> path, int maxSteps)
        {
            using ReachableMap map = ComputeReachable(agent, agent.GridMemberComponent.grid, origin, maxSteps);
            return map.TryGetPathToward(goal, path);
        }

        /// <summary>
        /// Resolves the redirect chain from stepping off <paramref name="from"/> onto
        /// <paramref name="chosen"/>. Appends every traversed tile (chosen first) to
        /// <paramref name="traversed"/> and returns the final landing tile. Stops on a cycle
        /// or when a redirect would push off-grid (staying on the last valid tile).
        ///
        /// This is the SINGLE place a redirect is ever resolved; every reachability/path query
        /// funnels through here so the rules stay in one spot.
        /// </summary>
        public static HexCoordinates ResolveRedirect(
            PathfindingAgentAspect agent, BattleGrid grid,
            HexCoordinates from, HexCoordinates chosen, List<HexCoordinates> traversed)
        {
            // The entry tile must itself be on-grid and standable for this agent. If it isn't, no
            // move happens (nothing is appended to `traversed`) and the agent stays on `from`.
            if (!grid.TryGetBattleCell(chosen, out BattleCellAspect chosenCell) ||
                !IsTraversable(agent, chosenCell))
                return from;

            HexCoordinates previous = from;
            HexCoordinates current = chosen;
            traversed.Add(current);

            using (HashSetPool<HexCoordinates>.Get(out var visited))
            {
                visited.Add(current);

                while (grid.TryGetBattleCell(current, out BattleCellAspect cell)
                       && TryRedirectOnce(agent, cell, previous, current, out HexCoordinates next))
                {
                    if (!visited.Add(next))
                        break; // redirect cycle

                    // A redirect — a slide today, a teleport tomorrow — may only place the agent on
                    // a tile that is on-grid AND standable for it. An off-grid or occupied target
                    // stops the chain on the last valid tile, so the agent is never left standing on
                    // a blocked tile (and an occupied redirecting tile can never be taken).
                    if (!grid.TryGetBattleCell(next, out BattleCellAspect nextCell))
                    {
                        traversed.Add(next);
                        current = next;
                        break;
                    }
                    
                    if (!IsTraversable(agent, nextCell))
                        break;

                    traversed.Add(next);
                    previous = current;
                    current = next;
                }
            }

            return current;
        }

        // Unit-cost BFS. Each dequeued tile expands its ring-1 neighbours; every accepted neighbour
        // is resolved through the redirect chain and recorded at its LANDING tile. FIFO + uniform
        // cost means the first time a tile is recorded it already has the minimum step count.
        private static void Flood(
            PathfindingAgentAspect agent, BattleGrid grid, HexCoordinates origin, int maxSteps,
            ReachableMap map)
        {
            using (ListPool<HexCoordinates>.Get(out var frontier))
            using (ListPool<HexCoordinates>.Get(out var traversed))
            {
                frontier.Add(origin);
                int head = 0;

                while (head < frontier.Count)
                {
                    HexCoordinates current = frontier[head++];
                    map.TryGetRawCost(current, out int cost);
                    if (cost >= maxSteps)
                        continue;

                    ReadOnlySpan<HexCoordinates> directions = HexOperations.Directions;
                    for (int i = 0; i < directions.Length; i++)
                    {
                        HexCoordinates neighbour = current + directions[i];
                        if (!grid.TryGetBattleCell(neighbour, out BattleCellAspect neighbourCell))
                            continue;
                        if (!IsTraversable(agent, neighbourCell))
                            continue;

                        traversed.Clear();
                        HexCoordinates landing = ResolveRedirect(agent, grid, current, neighbour, traversed);

                        if (!grid.TryGetBattleCell(landing, out _))
                            continue;
                        
                        // ResolveRedirect only ever returns an on-grid, standable tile (it stops the
                        // redirect chain before any blocked tile), so `landing` needs no re-check.
                        int newCost = cost + 1;
                        if (map.TryGetRawCost(landing, out int existing) && existing <= newCost)
                            continue;

                        map.Record(landing, newCost, new MovementStep(current, traversed.ToArray()));
                        frontier.Add(landing);
                    }
                }
            }
        }

        /// <summary>
        /// Reconstructs the tiles from <paramref name="origin"/> (exclusive) to
        /// <paramref name="goal"/> (inclusive) using BFS parent links, redirect slides included,
        /// and APPENDS them to <paramref name="path"/> (does not clear it). Returns whether a
        /// complete chain back to origin was found. Internal: consumers use
        /// <see cref="ReachableMap.TryGetPathFor"/>, which is O(1) against baked paths.
        /// </summary>
        internal static bool AppendReconstruct(
            HexCoordinates origin, HexCoordinates goal,
            Dictionary<HexCoordinates, MovementStep> cameFrom, List<HexCoordinates> path)
        {
            if (goal == origin)
                return true;
            if (!cameFrom.ContainsKey(goal))
                return false;

            int startCount = path.Count;
            HexCoordinates current = goal;

            while (current != origin && cameFrom.TryGetValue(current, out MovementStep step))
            {
                HexCoordinates[] tiles = step.traversed;
                for (int i = tiles.Length - 1; i >= 0; i--)
                    path.Add(tiles[i]);

                current = step.previous;
            }

            path.Reverse(startCount, path.Count - startCount);
            return current == origin;
        }

        public static bool IsTraversable(PathfindingAgentAspect agent, BattleCellAspect cell)
        {
            // Traversability is defined ENTIRELY by the agent's rules — no built-in gate. An
            // agent with no rules can cross anything; add a CellOccupancyRule to block tiles
            // held by other units (it also lets the agent pass back through its own tile).
            ReadOnlySpan<PathfindingTraversableRule> rules = agent.AgentRules;
            for (int i = 0; i < rules.Length; i++)
                if (rules[i] != null && !rules[i].CanTraverse(agent, cell))
                    return false;

            return true;
        }

        private static bool TryRedirectOnce(
            PathfindingAgentAspect agent, BattleCellAspect redirectCell,
            HexCoordinates directionOrigin, HexCoordinates to, out HexCoordinates redirected)
        {
            // PathfindingRedirectionIterator is a struct, and the generated
            // ForeachRedirectStatusComponent takes the iterator as an INTERFACE — passing a struct
            // there boxes it, and Process<T> mutates that box. We must therefore box ONCE up front
            // (declare the local as the interface type) and read the result back from that same box.
            // Reading a fresh struct local instead would lose every mutation (To / WasRedirected),
            // which is exactly why redirects silently never fired.
            IRedirectStatusComponentIterator iterator =
                new PathfindingRedirectionIterator(redirectCell, directionOrigin, to, agent);
            iterator.ForeachRedirectStatusComponent();

            var resolved = (PathfindingRedirectionIterator)iterator;
            redirected = resolved.To;
            return resolved.WasRedirected && resolved.To != to;
        }
    }
}
