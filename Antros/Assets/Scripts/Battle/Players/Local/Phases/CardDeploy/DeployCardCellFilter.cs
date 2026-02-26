using ATCG.Battle.Grids;
using ATCG.Battle.Players.Local.Phases.Filters;
using ATCG.HexGrids;

namespace ATCG.Battle.Players.Local.Phases.CardDeploy
{
    public class DeployCardCellFilter : ICellFilter
    {
        public void Initialize(BattleGrid battleGrid)
        {

        }

        public bool Accepts(BattleGrid grid, HexCoordinates coordinates)
        {
            if (grid.CanDeploy(coordinates))
                return true;

            return false;
        }
    }
}