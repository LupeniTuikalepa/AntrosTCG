using System.Collections.Generic;
using ATCG.Battle.Cards.Capacities.Interfaces;
using ATCG.Battle.Grids;
using ATCG.Cards;

namespace ATCG.Battle.Cards.Capacities
{
    public static class CapacityTargetExtensions
    {
        public static IEnumerable<ICapacityTarget> GetValidTargets<T>(this CapacityTargetType targetType, BattleCard<T> card, BattleCell battleCell)
            where T : GameCardData
        {
            bool lookForAllies = targetType.HasFlagFast(CapacityTargetType.Allies);
            bool lookForOpponents = targetType.HasFlagFast(CapacityTargetType.Opponents);
            bool lookForSelf = targetType.HasFlagFast(CapacityTargetType.Self);
            bool lookForCells = targetType.HasFlagFast(CapacityTargetType.Cells);
            foreach (var member in battleCell.Members)
            {
                if (member is not ICapacityTarget target)
                    continue;
                if(lookForSelf && target == card)
                    yield return target;
                else if (lookForAllies && target is IBattleCard ally && card.Player == ally.Player)
                    yield return target;
                else if (lookForOpponents && target is IBattleCard oppponent && card.Player != oppponent.Player)
                    yield return target;
            }

            if (lookForCells)
                yield return battleCell;
        }

        public static bool HasFlagFast(this CapacityTargetType value, CapacityTargetType flag)
        {
            return (value & flag) != 0;
        }
    }
}