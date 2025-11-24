using ATCG.HexGrids.Grids;

namespace ATCG.HexGrids.Shapes
{
    public interface IShapeBuilder
    {
        public void Build(HexGrid hexGrid);
    }
}