using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;

namespace ATCG.Battle
{
    public static class EntityActionsManager
    {
        public static void GetActionsFor(this EntityAddress address, List<IEntityAction> actions, IBattlePlayer player)
        {
            if (address.TryGetComponentRO(out MovementComponent movementComponent))
                actions.Add(new MoveAction(movementComponent.Speed));

            if (address.TryGetComponentRO(out BasicAttackerComponent attackerComponent))
                actions.Add(new PerformBasicAttackAction(attackerComponent.Strength, player));

            if (address.TryGetComponentRO(out CapacityCasterComponent component))
            {
                for (int i = 0; i < component.capacities.Length; i++)
                    actions.Add(new CastCapacityAction(component.capacities[i]));
            }
        }
    }
}