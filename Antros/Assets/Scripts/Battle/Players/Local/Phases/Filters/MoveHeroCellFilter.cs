using System.Collections.Generic;
using System.Linq;
using ATCG.Battle.Cards;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public class MoveHeroCellFilter : IBattleCellLookupFilter
    {
        private List<HexCoordinates> validCells = new List<HexCoordinates>();

        public void Initialize(BattleGrid grid, IBattleCard card)
        {
            validCells = ListPool<HexCoordinates>.Get();

            if(card is not HeroBattleCard hero)
                return;

            using (DictionaryPool<HexCoordinates, int>.Get(out Dictionary<HexCoordinates, int> dic))
            {
                Fill(grid, card.Coordinates, hero.Speed, dic);

                foreach ((HexCoordinates hexCoordinates, _) in dic)
                    validCells.Add(hexCoordinates);
            }

        }

        public bool Accepts(BattleGrid grid, HexCoordinates coordinates, IBattleCard battleCard) =>
            validCells.Contains(coordinates);

        public void Dispose(BattleGrid grid, IBattleCard card)
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

                if(cell.Members.Any())
                    continue;

                foundCells[neighbor] = budget;
                budget--;

                //Recursively check the neighbor cells
                Fill(grid, neighbor, budget, foundCells);
            }
        }
    }
}