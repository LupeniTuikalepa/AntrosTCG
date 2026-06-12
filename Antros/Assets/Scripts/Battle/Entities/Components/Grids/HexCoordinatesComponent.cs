using ATCG.Battle.Grids;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;

namespace ATCG.Battle.Entities.Components
{
    public struct HexCoordinatesComponent : IEntityComponent
    {
        public readonly BattleGrid grid;

        public HexCoordinates coordinates;

        public HexCoordinatesComponent(BattleGrid grid, HexCoordinates coordinates)
        {
            this.grid = grid;
            this.coordinates = coordinates;
        }
    }
}