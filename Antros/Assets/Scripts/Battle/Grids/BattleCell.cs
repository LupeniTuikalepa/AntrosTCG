using ATCG.HexGrids;

namespace ATCG.Battle.HexGrids
{
    public class BattleCell
    {
        public readonly BattleGrid battleGrid;
        public readonly HexCell cell;

        public BattleCell(BattleGrid battleGrid, HexCell cell)
        {
            this.battleGrid = battleGrid;
            this.cell = cell;
        }


    }
}