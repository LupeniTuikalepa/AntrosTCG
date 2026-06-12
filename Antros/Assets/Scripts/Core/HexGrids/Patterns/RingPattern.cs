using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.Grids.Patterns
{
    public readonly struct RingPattern : IHexPattern
    {
        private readonly int distance;

        public RingPattern(int distance)
        {
            this.distance = distance;
        }

        IEnumerable<HexCoordinates> IHexPattern.GetAll(HexCoordinates from) => from.GetRing(distance);
    }
}