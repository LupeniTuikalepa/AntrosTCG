using System.Collections.Generic;
using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    public static class PatternExtensions
    {
        public static HexPatternBuilder ToPatternBuilder(this CapacityPatternData data, HexCoordinates from)
        {
            HexPatternBuilder builder = new(from);
            if(CapacityManager.TryGetFor(data, out var container))
                container.AddToBuilder(data, ref builder);

            return builder;
        }
        public static HexPatternBuilder ToPatternBuilder(this CapacityPatternData[] datas, HexCoordinates from)
        {
            HexPatternBuilder builder = new(from);
            for (int i = 0; i < datas.Length; i++)
            {
                if(CapacityManager.TryGetFor(datas[i], out var container))
                    container.AddToBuilder(datas[i], ref builder);
            }

            return builder;
        }


        public static IEnumerable<BattleCellAspect> GetBattleCells(this HexPatternBuilder hexPatternBuilder, BattleGrid battleGrid)
        {
            foreach (HexCoordinates coordinate in hexPatternBuilder.GetCoordinates())
            {
                if(battleGrid.TryGetBattleCell(coordinate, out BattleCellAspect cell))
                    yield return cell;
            }
        }
    }
}