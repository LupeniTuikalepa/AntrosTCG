using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.Players.Local.Phases.Preview
{
    public interface ISelectionPatternPreview
    {
        HexPatternBuilder GetPreview(HexCoordinates coordinates);
    }
}