using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Components
{
    public struct BattleCellComponent : IEntityComponent
    {
        public HexCoordinates coordinates;

        public BattleCellComponent(HexCoordinates coordinates) : this()
        {
            this.coordinates = coordinates;
        }
    }
}