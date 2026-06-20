using System.Collections.Generic;
using System.Reflection.Emit;
using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.Grids
{
    public static class PatternExtensions
    {
        public static IEnumerable<BattleCellAspect> GetBattleCells<T>(this HexPatternBuilder<T> builder,
            BattleGrid battleGrid) where T : IHexPatternController
        {
            foreach (HexCoordinates coordinate in builder.GetCoordinates())
            {
                if (battleGrid.TryGetBattleCell(coordinate, out var battleCell))
                    yield return battleCell;
            }
        }
    }
}