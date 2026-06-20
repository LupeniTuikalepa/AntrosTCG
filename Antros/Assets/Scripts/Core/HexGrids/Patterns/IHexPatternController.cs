using ATCG.HexGrids.Grids;

namespace ATCG.HexGrids.Patterns
{
    public interface IHexPatternController
    {
        HexGrid HexGrid { get; }
        bool Blocks(HexCoordinates coordinates);
    }
}