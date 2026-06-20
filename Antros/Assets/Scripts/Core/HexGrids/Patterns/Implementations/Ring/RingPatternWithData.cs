using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct RingPatternWithData : IHexPattern<RingPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, RingPatternData data,
            TController controller)
            where TController : IHexPatternController
        {
            RingPattern pattern = new RingPattern(data.Distance);
            return pattern.GetAll(from, controller);
        }
    }
}