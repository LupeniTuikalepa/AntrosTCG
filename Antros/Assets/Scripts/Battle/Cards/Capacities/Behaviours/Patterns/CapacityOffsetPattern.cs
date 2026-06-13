using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public struct CapacityOffsetPattern : ICapacityHexPattern<OffsetsPatternData, OffsetsPattern>
    {
        public OffsetsPattern CreatePattern(OffsetsPatternData data) => new OffsetsPattern(data.Offset);
    }
}