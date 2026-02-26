using ATCG.HexGrids;

namespace ATCG.Battle.Players.Local.Phases.Filters
{
    public class MoveHeroCellFilter : SpreadCellFilter
    {
        public MoveHeroCellFilter(int distance, HexCoordinates center) : base(distance, center)
        {

        }

    }
}