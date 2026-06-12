using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;
using ATCG.Capacities;

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

            if (address.IsCapacityCasterAspect(out var aspect))
            {
                CapacityData[] capacities = aspect.CapacityCasterComponent.capacities;
                for (int i = 0; i < capacities.Length; i++)
                    actions.Add(new CastCapacityAction(capacities[i], aspect.HexCoordinatesComponent.coordinates));
            }
        }
    }
}