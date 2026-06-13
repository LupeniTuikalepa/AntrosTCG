using ATCG.Battle.Grids.Patterns;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public struct CapacitySpreadPattern : ICapacityHexPattern<SpreadCapacityPatternData, SpreadPattern>
    {
        public SpreadPattern CreatePattern(SpreadCapacityPatternData data) => new SpreadPattern(data.Distance);
    }
}