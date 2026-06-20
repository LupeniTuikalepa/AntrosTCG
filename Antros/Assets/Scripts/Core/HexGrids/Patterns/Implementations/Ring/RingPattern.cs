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

        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, TController controller) where TController : IHexPatternController
            => from.GetRing(distance);
    }
}