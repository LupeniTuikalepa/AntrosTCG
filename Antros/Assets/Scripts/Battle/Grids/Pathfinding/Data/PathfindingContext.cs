using System;
using System.Collections.Generic;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids
{
    public readonly struct PathfindingContext : IDisposable
    {
        public readonly Dictionary<HexCoordinates, int> costSoFar;
        public readonly Dictionary<HexCoordinates, HexCoordinates> cameFrom;
        public readonly List<PriorityHexCoordinates> frontier;
        public readonly int maxSteps;

        public PathfindingContext(int maxSteps)
        {
            this.maxSteps = maxSteps;

            costSoFar = DictionaryPool<HexCoordinates, int>.Get();
            cameFrom = DictionaryPool<HexCoordinates, HexCoordinates>.Get();
            frontier = ListPool<PriorityHexCoordinates>.Get();
        }

        public void Dispose()
        {
            DictionaryPool<HexCoordinates, int>.Release(costSoFar);
            DictionaryPool<HexCoordinates, HexCoordinates>.Release(cameFrom);
            ListPool<PriorityHexCoordinates>.Release(frontier);
        }
    }
}