using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.Grids.Patterns
{
    public readonly struct LinePattern : IHexPattern
    {
        private readonly HexCoordinates destination;

        public LinePattern(HexCoordinates destination)
        {
            this.destination = destination;
        }


        IEnumerable<HexCoordinates> IHexPattern.GetAll(HexCoordinates from) => from.GetLine(destination);
    }
}