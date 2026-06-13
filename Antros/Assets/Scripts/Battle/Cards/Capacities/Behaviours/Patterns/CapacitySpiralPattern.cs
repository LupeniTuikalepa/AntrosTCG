using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public struct CapacitySpiralPattern : ICapacityHexPattern<SpiralPatternData, SpiralPattern>
    {
        public SpiralPattern CreatePattern(SpiralPatternData data) => new SpiralPattern(data.Distance);
    }
}