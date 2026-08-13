using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players.Local;
using ATCG.Capacities;

namespace ATCG.Battle
{
    public static class EntityActionsManager
    {
        public static void GetActionsFor(this EntityAddress address, List<EntityAction> actions, LocalBattlePlayer player)
        {
            if (address.TryGetComponentRO(out MovementComponent movementComponent))
                actions.Add(new MoveAction(player, movementComponent.Speed));

            if (address.TryGetComponentRO(out BasicAttackerComponent attackerComponent))
                actions.Add(new PerformBasicAttackAction(player, attackerComponent.Strength));

            if (address.Is<CapacityCasterAspect>(out var aspect))
            {
	            HashSet<CapacityData> capacities = aspect.CapacityCasterComponent.capacities;
	            foreach (var capacityData in capacities)
	            {
                    actions.Add(new CastCapacityAction(player, capacityData, aspect.GridMemberComponent.coordinates));
	            }
            }
        }
    }
}