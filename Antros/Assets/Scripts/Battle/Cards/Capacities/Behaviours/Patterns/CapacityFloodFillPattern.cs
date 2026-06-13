using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public struct CapacityFloodFillPattern : ICapacityHexPattern<FloodFillPatternData, FloodFillPattern>
    {
        public FloodFillPattern CreatePattern(FloodFillPatternData data) => new FloodFillPattern(data.Distance);
    }
}