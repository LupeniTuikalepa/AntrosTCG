using System.Collections.Generic;
using System.Reflection.Emit;
using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.Grids
{
    public static class PatternExtensions
    {
        public static IEnumerable<BattleCellAspect> GetBattleCells(this HexPatternBuilder builder,
            BattleGrid battleGrid)
        {
            foreach (HexCoordinates coordinate in builder.GetCoordinates())
            {
                if (battleGrid.TryGetBattleCell(coordinate, out var battleCell))
                    yield return battleCell;
            }
        }
    }
}