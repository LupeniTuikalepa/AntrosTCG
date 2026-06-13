using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public struct CapacityRingPattern : ICapacityHexPattern<RingPatternData, RingPattern>
    {
        public RingPattern CreatePattern(RingPatternData data) => new RingPattern(data.Distance);
    }
}