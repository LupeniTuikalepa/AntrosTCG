using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct EverythingPattern : IHexPattern
    {
        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, TController controller) where TController : IHexPatternController
            => controller.HexGrid.CellsCoordinates;
    }
}