using ATCG.Battle.Grids.Patterns;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public struct SpreadPatternGenerator : IHexPatternGenerator<SpreadPatternData, SpreadPattern>
    {
        public SpreadPattern CreatePattern(SpreadPatternData data) => new SpreadPattern(data.Distance);
    }
}