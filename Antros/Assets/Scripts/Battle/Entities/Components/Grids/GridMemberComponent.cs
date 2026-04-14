using ATCG.HexGrids;
using ATCG.HexGrids.Grids;

namespace ATCG.Battle.Entities.Components
{
    public struct GridMemberComponent : IEntityComponent
    {
        public HexCoordinates Coordinates { get; private set; }

        public GridMemberComponent(HexGrid grid)
        {
            Coordinates = HexCoordinates.None;
        }


        public void SetCoordinates(HexCoordinates coordinates)
        {
            Coordinates = coordinates;
        }
    }
}