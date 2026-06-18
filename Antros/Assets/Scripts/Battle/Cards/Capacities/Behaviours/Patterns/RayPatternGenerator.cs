using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public struct RayPatternGenerator : IHexPatternGenerator<RayPatternData, RayPattern>
    {
        public RayPattern CreatePattern(RayPatternData data) => new RayPattern(data.Direction);
    }
}