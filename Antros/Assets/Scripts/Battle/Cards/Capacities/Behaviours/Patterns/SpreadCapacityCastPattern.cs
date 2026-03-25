using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Patterns;
using ATCG.Capacities.Data;
using ATCG.Cards;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.Cards.Capacities.Patterns
{
    //TODO
    public class SpreadCapacityCastPattern : ComplexCapacityCastPattern<SpreadPatternData, FloodFillPattern>
    {
        /*
        private static bool ValidateLineOfSight<T>(BattleCard<T> card, HexCoordinates origin, HexCoordinates coord, BattleCell cell)
            where T : GameCardData
        {
            HexCoordinates last = origin;
            foreach (HexCoordinates lineCoord in origin.GetLine(coord))
            {
                if(lineCoord == last)
                    continue;

                if (!cell.CanBeAttacked(card))
                    return false;
            }

            return true;
        }

        protected override bool ValidateCell<TCardData>(Capacity context, BattleCell battleCell)
        {
            if (!base.ValidateCell(context, battleCell))
                return false;

            HexCoordinates origin = context.origin;
            HexCoordinates cellCoordinates = battleCell.cell.coordinates;

            foreach (HexCoordinates lineCoord in origin.GetLine(cellCoordinates))
            {
                if(lineCoord == origin)
                    continue;

                if (!battleCell.CanBeAttacked(context.card))
                    return false;
            }

            return true;
        }

        protected override FloodFillPattern GetCellPattern<TCardData>(SpreadPatternData castPatternData,
            Capacity context)
        {
            return new FloodFillPattern(castPatternData.Distance, context.card.Coordinates);
        }*/

        protected override FloodFillPattern GetCellPattern(SpreadPatternData castData, Capacity context)
        {
            throw new System.NotImplementedException();
        }
    }
}