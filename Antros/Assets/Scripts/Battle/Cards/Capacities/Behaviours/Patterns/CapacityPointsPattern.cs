using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public struct CapacityPointsPattern : ICapacityHexPattern<PointsPatternData, PointsPattern>
    {
        public PointsPattern CreatePattern(PointsPatternData data) => new PointsPattern(data.Points);
    }
}