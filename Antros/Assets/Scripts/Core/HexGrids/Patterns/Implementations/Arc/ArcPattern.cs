using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns.Arc
{
    public readonly struct ArcPattern : IHexPattern
    {
        private readonly HexCoordinates casterCoord;
        private readonly HexCoordinates to;
        private readonly int radius;

        public ArcPattern(HexCoordinates casterCoord, HexCoordinates to, int radius)
        {
            this.casterCoord = casterCoord;
            this.to = to;
            this.radius = radius;
        }
        public IEnumerable<HexCoordinates> GetAll(HexCoordinates from, IHexPatternController controller) 
            => casterCoord.GetArc(to, radius);
    }
}