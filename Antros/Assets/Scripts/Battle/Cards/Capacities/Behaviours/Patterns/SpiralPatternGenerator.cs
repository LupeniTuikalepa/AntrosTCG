using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public struct SpiralPatternGenerator : IHexPatternGenerator<SpiralPatternData, SpiralPattern>
    {
        public SpiralPattern CreatePattern(SpiralPatternData data) => new SpiralPattern(data.Distance);
    }
}