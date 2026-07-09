using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities.Aspects;
using ATCG.Capacities.Data.Status;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids
{
    public readonly struct HexPathfinder : IDisposable
    {
        private readonly struct DefaultPathfinderController : IPathfinderController
        {
            private readonly HexCoordinates heroCoordinates;

            public DefaultPathfinderController(HexCoordinates heroCoordinates)
            {
                this.heroCoordinates = heroCoordinates;
            }

            bool IPathfinderController.CanTraverse(BattleCellAspect cell)
            {
                return cell.CanBeMovedOn() || cell.Coordinate == heroCoordinates;
            }

            int IPathfinderController.GetCost(HexCoordinates from, HexCoordinates to, BattleCellAspect cell) => 1;
            public bool TryRedirect(HexCoordinates from, HexCoordinates to, BattleGrid battleGrid, out HexCoordinates newCoordinates)
            {
                if (!battleGrid.TryGetBattleCell(to, out var toCellAspect))
                {
                    newCoordinates = HexCoordinates.None;
                    return false;
                }

                //TODO check good component
                if (toCellAspect.EntityAddress.HasStatusWithData<FreezeStatusData>())
                {
                    var direction = from.GetNormalizedDirection(to).NearestCardinal();

                    if (direction.X == 0 && direction.Y == 0)
                    {
                        newCoordinates = HexCoordinates.None;
                        return false;
                    }

                    var redirectedCoord = to + direction;
                    
                    if (battleGrid.TryGetBattleCell(redirectedCoord, out _) 
                        && TryRedirect(to, redirectedCoord, battleGrid, out var coord))
                    {
                        newCoordinates = coord;
                        return true;
                    }

                    newCoordinates = redirectedCoord;
                    return true;
                }
                newCoordinates = to;
                return false;
            }
        }

        private readonly int maxSteps;
        private readonly Dictionary<HexCoordinates, int> costSoFar;
        private readonly Dictionary<HexCoordinates, HexCoordinates> cameFrom;
        private readonly List<PriorityHexCoordinates> frontier;

        public HexPathfinder(int maxSteps = int.MaxValue)
        {
            this.maxSteps = maxSteps;
            costSoFar = DictionaryPool<HexCoordinates, int>.Get();
            cameFrom = DictionaryPool<HexCoordinates, HexCoordinates>.Get();
            frontier = ListPool<PriorityHexCoordinates>.Get();

        }

        public bool TryFindPath(
            HexCoordinates start,
            HexCoordinates goal,
            HexCoordinates heroCoordinates,
            List<HexCoordinates> path,
            BattleGrid battleGrid) 
                => TryFindPath(start, goal, path, battleGrid, new DefaultPathfinderController(heroCoordinates));


        public bool TryFindPath<TController>(
            HexCoordinates start,
            HexCoordinates goal,
            List<HexCoordinates> path,
            BattleGrid battleGrid,
            TController controller) where TController : IPathfinderController
        {
            costSoFar.Clear();
            cameFrom.Clear();
            frontier.Clear();
            
            if (!battleGrid.TryGetBattleCell(start, out BattleCellAspect startCell))
                return false;

            if (!battleGrid.TryGetBattleCell(goal, out BattleCellAspect goalCell))
                return false;

            bool goalIsFrozen = goalCell.EntityAddress.HasStatusWithData<FreezeStatusData>() ;

            frontier.Add(new PriorityHexCoordinates(start, 0));
            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                frontier.Sort();
                var current = frontier[0].coordinates;
                frontier.RemoveAt(0);

                if (current == goal)
                    break;

                if (!battleGrid.TryGetBattleCell(current, out _))
                    continue;

                foreach (HexCoordinates next in GetNeighbors(current, battleGrid))
                {
                    if (!battleGrid.TryGetBattleCell(next, out BattleCellAspect nextCell))
                        continue;

                    if (!controller.CanTraverse(nextCell))
                        continue;

                    
                    HexCoordinates actual = next;

                    bool nextIsGoal = next == goal && goalIsFrozen;

                    if (controller.TryRedirect(current, next, battleGrid, out HexCoordinates redirected)
                        && !nextIsGoal)
                    {
                        costSoFar[next] = int.MaxValue;
                        cameFrom[next] = current;

                        if (!battleGrid.TryGetBattleCell(redirected, out BattleCellAspect redirectedCell))
                            continue;

                        actual = redirected;
                        nextCell = redirectedCell;
                    }
                    int newCost = costSoFar[current] + controller.GetCost(current, actual, nextCell);

                    if (newCost > maxSteps)
                        continue;

                    if (!costSoFar.ContainsKey(actual) || newCost < costSoFar[actual])
                    {
                        Debug.Log($"Actual is start {actual == start}, Start {start}");
                        costSoFar[actual] = newCost;
                        int priority = newCost + actual.Distance(goal);
                        frontier.Add(new PriorityHexCoordinates(actual, priority));
                        cameFrom[actual] = current;
                    }
                }
            }

            if (!cameFrom.TryGetValue(goal, out var entryCase))
                return false;
            if (!goalIsFrozen)
                return ReconstructPath(start, goal, path);

            if (!controller.TryRedirect(entryCase, goal, battleGrid, out HexCoordinates actualGoal))
                return ReconstructPath(start, goal, path);

            if (!ReconstructPath(start, goal, path)) 
                return false;
            
            path.Add(actualGoal);
            return true;
        }

        private IEnumerable<HexCoordinates> GetNeighbors(
            HexCoordinates from,
            BattleGrid battleGrid)
        {
            for (var i = 0; i < HexOperations.Directions.Length; i++)
            {
                HexCoordinates neighbor = from + HexOperations.Directions[i];
                if (battleGrid.TryGetBattleCell(neighbor, out _))
                    yield return neighbor;
            }
        }
        
        private bool ReconstructPath(
            HexCoordinates start,
            HexCoordinates goal,
            List<HexCoordinates> path)
        {
            if (!cameFrom.ContainsKey(goal))
                return false;
            if(!cameFrom.ContainsKey(start))
                return false;

            Debug.Log($"Reconstruct path from {start} to {goal}");
            HexCoordinates current = goal;
            while (current != start)
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Reverse();
            return path.Count > 0;
        }

        void IDisposable.Dispose()
        {
            DictionaryPool<HexCoordinates, int>.Release(costSoFar);
            DictionaryPool<HexCoordinates, HexCoordinates>.Release(cameFrom);
            ListPool<PriorityHexCoordinates>.Release(frontier);
        }
    }
}