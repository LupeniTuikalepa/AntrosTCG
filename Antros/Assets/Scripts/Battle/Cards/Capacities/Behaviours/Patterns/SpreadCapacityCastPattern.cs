using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    //TODO
    public class SpreadCapacityCastPattern : ComplexCapacityCastPattern<SpreadPatternData, SpreadPattern>
    {
        protected override SpreadPattern GetCellPattern(SpreadPatternData castData, CapacityContext context)
        {
            return new SpreadPattern(context.castPoint, castData.Distance);
        }

        protected override bool ValidateCell(CapacityContext context, BattleCellAspect battleCellAspect)
        {
            if (!base.ValidateCell(context, battleCellAspect))
                return false;

            HexCoordinates castPoint = context.castPoint;
            HexCoordinates cellCoordinates = battleCellAspect.Coordinate;

            foreach (HexCoordinates lineCoord in castPoint.GetLine(cellCoordinates))
            {
                if (lineCoord == castPoint)
                    continue;

                if (!battleCellAspect.CanBeAttacked(context.card))
                    return false;
            }

            return true;
        }
    }
}