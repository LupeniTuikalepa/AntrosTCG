using System;
using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Patterns
{
    public readonly struct FloodFillPattern : ICellPattern
    {
        public readonly int distance;
        public readonly HexCoordinates center;


        public FloodFillPattern(HexCoordinates center, int distance)
        {
            this.center = center;
            this.distance = distance;
        }
        
        IEnumerable<HexCoordinates> ICellPattern.GetAll()
        {
            using (DictionaryPool<HexCoordinates, int>.Get(out Dictionary<HexCoordinates, int> dic))
            {
                FloodFill(center, distance, dic);

                foreach ((HexCoordinates hexCoordinates, _) in dic)
                    yield return hexCoordinates;
            }
        }

        private void FloodFill(HexCoordinates coordinates, int budget, Dictionary<HexCoordinates, int> foundCells)
        {
            if (budget <= 0)
                return;

            foreach (HexCoordinates neighbor in coordinates.GetRing(1))
            {
                if (foundCells.TryGetValue(neighbor, out int lastBudget))
                    //Someone already found the cell with the same budget or more, so no need to check it
                    if (lastBudget >= budget)
                        continue;

                foundCells[neighbor] = budget;
                budget--;

                //Recursively check the neighbor cells
                FloodFill(neighbor, budget, foundCells);
            }
        }
    }
}