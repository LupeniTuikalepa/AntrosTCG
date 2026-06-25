using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct EverythingPattern : IHexPattern
    {
        public IEnumerable<HexCoordinates> GetAll(HexCoordinates from, IHexPatternController controller)
            => controller.HexGrid.CellsCoordinates;
    }
}