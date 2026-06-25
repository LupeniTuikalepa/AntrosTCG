using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct RingPattern : IHexPattern
    {
        private readonly int distance;

        public RingPattern(int distance)
        {
            this.distance = distance;
        }

        public IEnumerable<HexCoordinates> GetAll(HexCoordinates from, IHexPatternController controller)
            => from.GetRing(distance);
    }
}