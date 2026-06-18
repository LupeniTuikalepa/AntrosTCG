using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Cards.Capacities.Behaviours.Mapping;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    public static class PatternExtensions
    {
        public static HexPatternBuilder ToPatternBuilder(this PatternData data, HexCoordinates from)
        {
            HexPatternBuilder builder = new(from);
            if(BattleDataMapper.TryGetFor(data, out var container))
                container.AddToBuilder(data, ref builder);

            return builder;
        }
        public static HexPatternBuilder ToPatternBuilder(this PatternData[] datas, HexCoordinates from)
        {
            HexPatternBuilder builder = new(from);
            for (int i = 0; i < datas.Length; i++)
            {
                if(BattleDataMapper.TryGetFor(datas[i], out var container))
                    container.AddToBuilder(datas[i], ref builder);
            }

            return builder;
        }

        public static HexPatternBuilder WithPatternData(this HexPatternBuilder builder, PatternData data)
        {
            if(BattleDataMapper.TryGetFor(data, out var container))
                container.AddToBuilder(data, ref builder);
            
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