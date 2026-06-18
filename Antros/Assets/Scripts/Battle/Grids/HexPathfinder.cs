using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using CollectionDebugger.Core;
using UnityEngine;

namespace ATCG.Battle.Grids
{
    public static class HexPathfinder
    {
        private static readonly Dictionary<HexCoordinates, int> CostSoFar;
        private static readonly Dictionary<HexCoordinates, HexCoordinates> CameFrom;
        private static readonly PriorityQueue<HexCoordinates, int> Frontier;
        private static readonly HexCoordinates[] Directions =
        {
            new(1, 0), new(1, -1), new(0, -1),
            new(-1, 0), new(-1, 1), new(0, 1)
        };
        
        static HexPathfinder()
        {
            Frontier = new PriorityQueue<HexCoordinates, int>();
            CameFrom = new Dictionary<HexCoordinates, HexCoordinates>();
            CostSoFar = new Dictionary<HexCoordinates, int>();
        }
        
        public static List<HexCoordinates> FindPath(
            HexCoordinates start,
            HexCoordinates goal,
            BattleGrid battleGrid,
            Func<BattleCellAspect, bool> filter = null,
            int maxSteps = int.MaxValue)
        {
            filter ??= aspect => aspect.CanBeMovedOn();
            Frontier.Clear();
            CameFrom.Clear();
            CostSoFar.Clear();

            Frontier.Enqueue(start, 0);
            CameFrom[start] = start;
            CostSoFar[start] = 0;
            Debug.Log($"[HexPathfinder] Start: {start}, Goal: {goal}");
            while (Frontier.Count > 0)
            {
                HexCoordinates current = Frontier.Dequeue();

                if (current.Equals(goal))
                    break;
                foreach (HexCoordinates next in GetNeighbors(current, battleGrid, filter))
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

        private static IEnumerable<HexCoordinates> GetNeighbors(HexCoordinates coord, BattleGrid battleGrid, Func<BattleCellAspect, bool> filter)
        {
            List<HexCoordinates> reachable = new();
            
            foreach (HexCoordinates dir in Directions)
            {
                HexCoordinates neighbor = coord + dir;

                if (!battleGrid.TryGetBattleCell(neighbor, out BattleCellAspect cell))
                    continue;

                if (filter(cell))
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
            int safety = 0;
            while (!current.Equals(start))
            {
                Debug.Log($"[Reconstruct] step {safety}: {current} → {cameFrom[current]}");
                path.Add(current);
                current = cameFrom[current];
    
                if (++safety > 100)
                {
                    Debug.LogError("[Reconstruct] Boucle infinie détectée !");
                    break;
                }
            }

            path.Reverse();
            return path;
        }
    }
}