using System;
using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Status.Forst;
using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using CollectionDebugger.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids
{
    public readonly struct HexPathfinder : IDisposable
    {
        private struct DefaultPathfinderController : IPathfinderController
        {
            bool IPathfinderController.CanTraverse(BattleCellAspect cell) => cell.CanBeMovedOn();

            int IPathfinderController.GetCost(HexCoordinates from, HexCoordinates to, BattleCellAspect cell) => 1;
            public bool TryRedirect(HexCoordinates from, BattleCellAspect toCellAspect, out HexCoordinates newCoordinates)
            {
                //TODO check good component
                if (toCellAspect.EntityAddress.HasComponent<FrostStatusComponent>())
                {
                    var to = toCellAspect.Coordinate;
                    var direction = from.GetDirection(to);
                    newCoordinates = to + direction;
                    return true;
                }
                newCoordinates = HexCoordinates.None;
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

        public bool FindPath(
            HexCoordinates start,
            HexCoordinates goal,
            List<HexCoordinates> path,
            BattleGrid battleGrid)
            => FindPath(start, goal, path, battleGrid, new DefaultPathfinderController());

        public bool FindPath<TController>(
    HexCoordinates start,
    HexCoordinates goal,
    List<HexCoordinates> path,
    BattleGrid battleGrid,
    TController controller) where TController : IPathfinderController
{
    costSoFar.Clear();
    cameFrom.Clear();
    frontier.Clear();

    HexCoordinates actualGoal = goal;
    
    if (battleGrid.TryGetBattleCell(goal, out BattleCellAspect goalCell) 
        && controller.TryRedirect(start, goalCell, out HexCoordinates redirectedGoal)) 
        actualGoal = redirectedGoal;

    frontier.Add(new PriorityHexCoordinates(start, 0));
    cameFrom[start] = start;
    costSoFar[start] = 0;

    while (frontier.Count > 0)
    {
        frontier.Sort();
        var current = frontier[0].coordinates;
        frontier.RemoveAt(0);

        if (current == actualGoal)
            break;

        foreach (HexCoordinates next in GetNeighbors(current, battleGrid))
        {
            if (!battleGrid.TryGetBattleCell(next, out BattleCellAspect nextCell))
                continue;

            HexCoordinates actual = next;

            if (controller.TryRedirect(current, nextCell, out HexCoordinates redirected))
            {
                costSoFar[next] = int.MaxValue;
                cameFrom[next] = current;

                if (!battleGrid.TryGetBattleCell(redirected, out BattleCellAspect redirectedCell))
                    continue;

                if (!controller.CanTraverse(redirectedCell) && redirected != actualGoal)
                    continue;

                actual = redirected;
                nextCell = redirectedCell;
            }

            int newCost = costSoFar[current] + controller.GetCost(current, actual, nextCell);

            if (newCost > maxSteps)
                continue;

            if (!costSoFar.ContainsKey(actual) || newCost < costSoFar[actual])
            {
                costSoFar[actual] = newCost;
                int priority = newCost + actual.Distance(actualGoal);
                frontier.Add(new PriorityHexCoordinates(actual, priority));
                cameFrom[actual] = current;
            }
        }
    }
    return ReconstructPath(start, actualGoal, path);
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

        private bool IsCoordinatesValid<TController>(HexCoordinates from,
            HexCoordinates goal,
            BattleGrid battleGrid,
            TController controller)
            where TController : IPathfinderController
        {
            if (from == goal)
                return true;

            if (!battleGrid.TryGetBattleCell(goal, out BattleCellAspect cell))
                return false;

            if (!controller.CanTraverse(cell))
                return false;

            return true;
        }

        private bool ReconstructPath(
            HexCoordinates start,
            HexCoordinates goal,
            List<HexCoordinates> path)
        {
            if (!cameFrom.ContainsKey(goal))
                return false;

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