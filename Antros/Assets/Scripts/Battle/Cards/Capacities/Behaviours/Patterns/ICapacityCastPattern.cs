using System.Collections.Generic;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public interface ICapacityCastPattern : ICapacityBehaviour<ICapacityCastPatternData>
    {
        void FillTargetedCells(ICapacityCastPatternData castData, in CapacityContext context,
            HashSet<BattleCellAspect> targetedCells);
    }

    public interface ICapacityCastPattern<in T> : ICapacityCastPattern where T : ICapacityCastPatternData
    {
        bool ICapacityBehaviour<ICapacityCastPatternData>.Accepts(ICapacityCastPatternData data)
        {
            return data is T;
        }

        void ICapacityCastPattern.FillTargetedCells(ICapacityCastPatternData castData, in CapacityContext context,
            HashSet<BattleCellAspect> targetedCells)
        {
            if (castData is T t)
                FillTargetedCells(t, context, targetedCells);
        }

        void FillTargetedCells(T castData, in CapacityContext context, HashSet<BattleCellAspect> targetedCells);
    }
}