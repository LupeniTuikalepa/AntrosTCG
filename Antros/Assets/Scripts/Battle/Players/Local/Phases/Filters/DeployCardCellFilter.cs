using ATCG.Battle.Cards;
using ATCG.Battle.Grids;
using ATCG.HexGrids;

namespace ATCG.Battle.Players.Local.Phases
{
    public class DeployCardCellFilter : IBattleCellLookupFilter
    {
        public bool Accepts(BattleGrid grid, HexCoordinates coordinates, IBattleCard battleCard)
        {
            if (grid.CanDeploy(coordinates))
                return true;

            return false;
        }
    }
}