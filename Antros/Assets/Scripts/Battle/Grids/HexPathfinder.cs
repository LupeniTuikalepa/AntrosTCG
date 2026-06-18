using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using CollectionDebugger.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids
{
    public readonly struct HexPathfinder : IDisposable
    {
        private readonly int maxSteps;
        private readonly Dictionary<HexCoordinates, int> costSoFar;
        private readonly Dictionary<HexCoordinates, HexCoordinates> cameFrom;
        private readonly PriorityQueue<HexCoordinates, int> frontier;
        private static readonly HexCoordinates[] Directions =
        {
            new(1, 0), new(1, -1), new(0, -1),
            new(-1, 0), new(-1, 1), new(0, 1)
        };

        public HexPathfinder(int maxSteps = int.MaxValue)
        {
            this.maxSteps = maxSteps;
            costSoFar = DictionaryPool<HexCoordinates, int>.Get();
            cameFrom = DictionaryPool<HexCoordinates, HexCoordinates>.Get();
            frontier = new PriorityQueue<HexCoordinates, int>();
        }

        public List<HexCoordinates> FindPath(
            HexCoordinates start,
            HexCoordinates goal,
            BattleGrid battleGrid,
            Func<BattleCellAspect, bool> filter = null)
        {
            filter ??= aspect => aspect.CanBeMovedOn();

            frontier.Enqueue(start, 0);
            cameFrom[start] = start;
            costSoFar[start] = 0;
            Debug.Log($"[HexPathfinder] Start: {start}, Goal: {goal}");
            while (frontier.Count > 0)
            {
                HexCoordinates current = frontier.Dequeue();

                if (current.Equals(goal))
                    break;
                foreach (HexCoordinates next in GetNeighbors(current, battleGrid, filter))
                {
                    int newCost = costSoFar[current] + 1;

                    if (newCost > maxSteps)
                        continue;

                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        int priority = newCost + HexDistance(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
            return ReconstructPath(cameFrom, start, goal);
        }

        private IEnumerable<HexCoordinates> GetNeighbors(HexCoordinates coord, BattleGrid battleGrid, Func<BattleCellAspect, bool> filter)
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

        private int HexDistance(HexCoordinates a, HexCoordinates b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return (Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dx + dy)) / 2;
        }

        private List<HexCoordinates> ReconstructPath(
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

        public void Dispose()
        {
            DictionaryPool<HexCoordinates, int>.Release(costSoFar);
            DictionaryPool<HexCoordinates, HexCoordinates>.Release(cameFrom);
        }
    }
}