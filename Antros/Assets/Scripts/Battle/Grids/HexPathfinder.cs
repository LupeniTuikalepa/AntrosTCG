using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    public static class HexPathfinder
    {
        private static readonly Dictionary<HexCoordinates, int> CostSoFar;
        private static readonly Dictionary<HexCoordinates, HexCoordinates> CameFrom;
        private static readonly PriorityQueue<HexCoordinates, int> Frontier;

        static HexPathfinder()
        {
            Frontier = new PriorityQueue<HexCoordinates, int>();
            CameFrom = new Dictionary<HexCoordinates, HexCoordinates>();
            CostSoFar = new Dictionary<HexCoordinates, int>();
        }
        
        public static List<HexCoordinates> FindPath(
            HexCoordinates start,
            HexCoordinates goal,
            HexPatternBuilder builder,
            BattleGrid battleGrid,
            int maxSteps = 1)
        {
            Frontier.Clear();
            CameFrom.Clear();
            CostSoFar.Clear();
            
            Frontier.Enqueue(start, 0);
            CameFrom[start] = start;
            CostSoFar[start] = 0;

            while (Frontier.Count > 0)
            {
                HexCoordinates current = Frontier.Dequeue();

                if (current.Equals(goal))
                    break;

                foreach (HexCoordinates next in GetNeighbors(current, builder, battleGrid))
                {
                    int newCost = CostSoFar[current] + 1;

                    if (newCost > maxSteps)
                        continue;

                    if (!CostSoFar.ContainsKey(next) || newCost < CostSoFar[next])
                    {
                        CostSoFar[next] = newCost;
                        int priority = newCost + HexDistance(next, goal);
                        Frontier.Enqueue(next, priority);
                        CameFrom[next] = current;
                    }
                }
            }

            return ReconstructPath(CameFrom, start, goal);
        }

        private static IEnumerable<HexCoordinates> GetNeighbors(
            HexCoordinates coord,
            HexPatternBuilder builder,
            BattleGrid battleGrid)
        {
            List<HexCoordinates> reachable = new();
            
            foreach (HexCoordinates neighbor in builder.GetCoordinates())
            {
                if (!battleGrid.TryGetBattleCell(neighbor, out BattleCellAspect cell))
                    continue;

                if (cell.CanBeMovedOn())
                    reachable.Add(neighbor);
            }
            

            return reachable;
        }

        private static int HexDistance(HexCoordinates a, HexCoordinates b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dx + dy)) / 2;
        }

        private static List<HexCoordinates> ReconstructPath(
            Dictionary<HexCoordinates, HexCoordinates> cameFrom,
            HexCoordinates start,
            HexCoordinates goal)
        {
            var path = new List<HexCoordinates>();

            if (!cameFrom.ContainsKey(goal))
                return path;

            HexCoordinates current = goal;
            while (!current.Equals(start))
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Reverse();
            return path;
        }
    }
}