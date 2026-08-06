using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
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
    /// does not cost extra speed. A single unit-cost BFS therefore yields both the reachable
    /// set (for the two highlight rings) and, via parent links, the path to any reachable tile
    /// (for "fast travel"): step-by-step and fast travel are the same computation.
    /// </summary>
    public static class HexPathfinder
    {
        /// <summary>
        /// Resolves the redirect chain from stepping off <paramref name="from"/> onto
        /// <paramref name="chosen"/>. Appends every traversed tile (chosen first) to
        /// <paramref name="traversed"/> and returns the final landing tile. Stops on a cycle
        /// or when a redirect would push off-grid (staying on the last valid tile).
        /// </summary>
        public static HexCoordinates ResolveRedirect(
            PathfindingAgentAspect agent, BattleGrid grid,
            HexCoordinates from, HexCoordinates chosen, List<HexCoordinates> traversed)
        {
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
                    if (!grid.TryGetBattleCell(next, out _))
                        break; // pushed off-grid: stop on the last valid tile

                    traversed.Add(next);
                    previous = current;
                    current = next;
                }
            }

            return current;
        }

        /// <summary>
        /// Floods every tile reachable within <paramref name="maxSteps"/> steps from
        /// <paramref name="origin"/>. Fills <paramref name="costSoFar"/> (step cost to each
        /// tile, origin = 0) and <paramref name="cameFrom"/> (parent link per tile).
        /// </summary>
        public static void GetReachable(
            PathfindingAgentAspect agent, BattleGrid grid, HexCoordinates origin, int maxSteps,
            Dictionary<HexCoordinates, int> costSoFar,
            Dictionary<HexCoordinates, MovementStep> cameFrom)
        {
            costSoFar[origin] = 0;
            if (maxSteps <= 0)
                return;

            using (ListPool<HexCoordinates>.Get(out var frontier))
            using (ListPool<HexCoordinates>.Get(out var traversed))
            {
                frontier.Add(origin);
                int head = 0;

                while (head < frontier.Count)
                {
                    HexCoordinates current = frontier[head++];
                    int cost = costSoFar[current];
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

                        // The tile the unit actually ends up on must itself be standable.
                        if (!grid.TryGetBattleCell(landing, out BattleCellAspect landingCell) || !IsTraversable(agent, landingCell))
                            continue;

                        int newCost = cost + 1;
                        if (costSoFar.TryGetValue(landing, out int existing) && existing <= newCost)
                            continue;

                        costSoFar[landing] = newCost;
                        cameFrom[landing] = new MovementStep(current, traversed.ToArray());
                        frontier.Add(landing);
                    }
                }
            }
        }

        public static bool TryBuildPath(
            HexCoordinates origin, 
            HexCoordinates goal, 
            PathfindingAgentAspect agent , 
            List<HexCoordinates> path, 
            int maxSteps = int.MaxValue)
        {
            using (DictionaryPool<HexCoordinates, int>.Get(out var costSoFar))
            using (DictionaryPool<HexCoordinates, MovementStep>.Get(out var cameFrom))
            {
                path.Add(origin);
                GetReachable(agent, agent.GridMemberComponent.grid, origin, maxSteps, costSoFar, cameFrom);

                // Nowhere to go from here.
                if (cameFrom.Count == 0)
                    return false;
                
                
                if (!costSoFar.TryGetValue(goal, out int goalCost) || goalCost <= 0)
                    return false;

                return TryBuildPath(origin, goal, cameFrom, path);
            }

               
                
        }
        
        /// <summary>
        /// Reconstructs the tiles from <paramref name="origin"/> (exclusive) to
        /// <paramref name="goal"/> (inclusive) using BFS parent links, redirect slides
        /// included. Appends to <paramref name="path"/> (does not clear it) and returns
        /// whether a complete chain back to origin was found.
        /// </summary>
        public static bool TryBuildPath(
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
            var iterator = new PathfindingRedirectionIterator(redirectCell, directionOrigin, to, agent);
            iterator.ForeachRedirectStatusComponent();
            redirected = iterator.To;
            return iterator.WasRedirected && iterator.To != to;
        }
    }
}
