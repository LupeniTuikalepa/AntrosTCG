using System.Collections.Generic;
using ATCG.Battle.Grids;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Patterns
{
    public interface ICapacityCastPattern : ICapacityBehaviour<ICapacityCastPatternData>
    {
        void FillTargetedCells(ICapacityCastPatternData castData, in Capacity context, HashSet<BattleCell> targetedCells);
    }
    public interface ICapacityCastPattern<in T> : ICapacityCastPattern where T : ICapacityCastPatternData
    {
        bool ICapacityBehaviour<ICapacityCastPatternData>.Accepts(ICapacityCastPatternData data) => data is T;

        void ICapacityCastPattern.FillTargetedCells(ICapacityCastPatternData castData, in Capacity context, HashSet<BattleCell> targetedCells)
        {
            if(castData is T t)
                FillTargetedCells(t, context, targetedCells);
        }

        void FillTargetedCells(T castData, in Capacity context, HashSet<BattleCell> targetedCells);
    }
}