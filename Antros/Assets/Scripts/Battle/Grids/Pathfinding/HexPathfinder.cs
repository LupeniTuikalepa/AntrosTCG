using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids
{
    public readonly struct HexPathfinder : IDisposable
    {
        private struct DefaultPathfinderController : IPathfinderController
        {
            bool IPathfinderController.CanTraverse(BattleCellAspect cell) => cell.CanBeMovedOn();

            int IPathfinderController.GetCost(HexCoordinates from, HexCoordinates to, BattleCellAspect cell) => 1;
            public bool TryRedirect(HexCoordinates from, BattleCellAspect to, out HexCoordinates newCoordinates)
            {
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

                foreach (HexCoordinates next in GetNeighbors(current, goal, battleGrid, controller))
                {

                    if (!battleGrid.TryGetBattleCell(next, out BattleCellAspect cell))
                        continue;

                    int newCost = costSoFar[current] + controller.GetCost(current, next, cell);

                    if (newCost > maxSteps)
                        continue;

                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        int priority = newCost + next.Distance(goal);
                        frontier.Add(new PriorityHexCoordinates(next, priority));
                        cameFrom[next] = current;
                    }

                }
            }
            return ReconstructPath(start, goal, path);
        }

        private IEnumerable<HexCoordinates> GetNeighbors<TController>(
            HexCoordinates from,
            HexCoordinates goal,
            BattleGrid battleGrid,
            TController controller)
            where TController : IPathfinderController
        {
            if (!battleGrid.TryGetBattleCell(from, out BattleCellAspect fromCell))
                yield break;

            if (controller.TryRedirect(from, fromCell, out var newCoordinates)
                && IsCoordinatesValid(newCoordinates, goal, battleGrid, controller))
                yield return newCoordinates;

            else
            {
                for (var i = 0; i < HexOperations.Directions.Length; i++)
                {

                    var dir = HexOperations.Directions[i];
                    HexCoordinates neighbor = from + dir;

                    if (IsCoordinatesValid(neighbor, goal, battleGrid, controller))
                        yield return neighbor;
                }
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

            if (!battleGrid.TryGetBattleCell(from, out BattleCellAspect cell))
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