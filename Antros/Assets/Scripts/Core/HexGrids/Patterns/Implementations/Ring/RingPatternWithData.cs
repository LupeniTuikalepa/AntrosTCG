using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct RingPatternWithData : IHexPattern<RingPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll(RingPatternData data, HexCoordinates from, IHexPatternController controller)
        {
            RingPattern pattern = new RingPattern(data.Distance);
            return pattern.GetAll(from, controller);
        }
    }
}