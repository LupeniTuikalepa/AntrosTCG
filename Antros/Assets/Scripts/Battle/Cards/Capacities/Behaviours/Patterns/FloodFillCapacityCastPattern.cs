using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public class FloodFillCapacityCastPattern : ComplexCapacityCastPattern<FloodFillPatternData, FloodFillPattern>
    {
        protected override FloodFillPattern GetCellPattern(FloodFillPatternData castData, CapacityContext context)
        {
            return new FloodFillPattern(context.castPoint, castData.Distance);
        }
    }
}