using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;

namespace ATCG.Battle.Grids.Patterns
{
    public readonly struct EverythingPattern : IHexPattern
    {
        private readonly HexGrid hexGrid;

        public EverythingPattern(HexGrid hexGrid)
        {
            this.hexGrid = hexGrid;
        }

        public IEnumerable<HexCoordinates> GetAll(HexCoordinates from) => hexGrid.CellsCoordinates;
    }
}