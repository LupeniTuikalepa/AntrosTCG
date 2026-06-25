using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct SpiralPattern : IHexPattern
    {
        private readonly int distance;

        public SpiralPattern(int distance)
        {
            this.distance = distance;
        }

        public IEnumerable<HexCoordinates> GetAll(HexCoordinates from, IHexPatternController controller)
            => from.GetSpiral(distance);
    }
}