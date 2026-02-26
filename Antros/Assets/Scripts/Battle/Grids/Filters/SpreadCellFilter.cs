using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases.Filters
{
    public class SpreadCellFilter : ICellFilter
    {
        private List<HexCoordinates> validCells = new List<HexCoordinates>();

        public readonly int distance;
        public readonly HexCoordinates center;

        public SpreadCellFilter(int distance, HexCoordinates center)
        {
            this.center = center;
            this.distance = distance;
        }

        public void Initialize(BattleGrid grid)
        {
            validCells = ListPool<HexCoordinates>.Get();

            using (DictionaryPool<HexCoordinates, int>.Get(out Dictionary<HexCoordinates, int> dic))
            {
                Fill(grid, center, distance, dic);

                foreach ((HexCoordinates hexCoordinates, _) in dic)
                {
                    validCells.Add(hexCoordinates);
                }
            }

        }

        public bool Accepts(BattleGrid grid, HexCoordinates coordinates) =>
            validCells.Contains(coordinates);

        public void Dispose(BattleGrid grid)
        {
            ListPool<HexCoordinates>.Release(validCells);
        }

        private void Fill(BattleGrid grid, HexCoordinates coordinates, int budget, Dictionary<HexCoordinates, int> foundCells)
        {
            if(budget <= 0)
                return;

            HexGrid hexGrid = grid.Grid;
            foreach (HexCoordinates neighbor in hexGrid.GetRing(coordinates, 1))
            {
                if (foundCells.TryGetValue(neighbor, out int lastBudget))
                {
                    //Someone already found the cell with the same budget or more, so no need to check it
                    if(lastBudget >= budget)
                        continue;
                }

                if (!hexGrid.TryGetCell(neighbor, out HexCell cell))
                    continue;

                if(ValidateCell(cell))
                    continue;

                foundCells[neighbor] = budget;
                budget--;

                //Recursively check the neighbor cells
                Fill(grid, neighbor, budget, foundCells);
            }
        }

        protected virtual bool ValidateCell(HexCell cell) => cell.Members.Any();
    }
}