using System;
using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Patterns
{
    public struct FloodFillPattern : ICellPattern
    {
        public Func<HexCoordinates, bool> ValidateCell { get; set; }

        public readonly int distance;
        public readonly HexCoordinates center;


        public FloodFillPattern(HexCoordinates center, int distance)
        {
            this.center = center;
            this.distance = distance;
            ValidateCell = null;
        }

        public void Evaluate(List<HexCoordinates> coordinatesList)
        {
            using (DictionaryPool<HexCoordinates, int>.Get(out Dictionary<HexCoordinates, int> dic))
            {
                FloodFill(center, distance, dic);

                foreach ((HexCoordinates hexCoordinates, _) in dic)
                    coordinatesList.Add(hexCoordinates);
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

                if (ValidateCell != null && !ValidateCell(neighbor))
                    continue;

                foundCells[neighbor] = budget;
                budget--;

                //Recursively check the neighbor cells
                FloodFill(neighbor, budget, foundCells);
            }
        }
    }
}