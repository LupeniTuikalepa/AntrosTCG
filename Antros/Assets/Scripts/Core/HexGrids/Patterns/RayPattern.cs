using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.Grids.Patterns
{
    public readonly struct RayPattern : IHexPattern
    {
        private readonly HexCoordinates direction;

        public RayPattern(HexCoordinates direction)
        {
            this.direction = direction;
        }

        IEnumerable<HexCoordinates> IHexPattern.GetAll(HexCoordinates from) => from.GetLine(from + direction);
    }
}