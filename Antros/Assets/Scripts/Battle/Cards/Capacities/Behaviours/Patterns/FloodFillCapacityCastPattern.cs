using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Patterns
{
    public class FloodFillCapacityCastPattern : ComplexCapacityCastPattern<FloodFillPatternData, FloodFillPattern>
    {
        protected override FloodFillPattern GetCellPattern(FloodFillPatternData castData, Capacity context)
        {
            return default;
            //TODO
            //return new FloodFillPattern(castPatternData.Distance, context.);
        }

    }
}