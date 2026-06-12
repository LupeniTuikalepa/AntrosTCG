using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.Grids.Patterns
{
    public readonly struct SpiralPattern : IHexPattern
    {
        private readonly int distance;

        public SpiralPattern(int distance)
        {
            this.distance = distance;
        }

        IEnumerable<HexCoordinates> IHexPattern.GetAll(HexCoordinates from) => from.GetSpiral(distance);
    }
}