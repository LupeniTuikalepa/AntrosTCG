using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class SelectEntityActionPhase : PhaseCompletionSource<IEntityAction>
    {
        public readonly IReadOnlyList<IEntityAction> actions;
        public readonly EntityAddress entityAddress;

        public static async Awaitable<IEntityAction> RunPhaseFor(EntityAddress address)
        {
            using (ListPool<IEntityAction>.Get(out var actions))
            {
                if (address.TryGetComponentRO(out MovementComponent movementComponent))
                    actions.Add(new MoveAction(movementComponent.Speed));

                if (address.TryGetComponentRO(out BasicAttackerComponent attackerComponent))
                    actions.Add(new PerformBasicAttackAction(attackerComponent.Strength));

                if (address.TryGetComponentRO(out CapacityCasterComponent component))
                {
                    for (int i = 0; i < component.capacities.Length; i++)
                        actions.Add(new CastCapacityAction(component.capacities[i]));
                }

                if (actions.Count > 0)
                {
                    PhaseResult<IEntityAction> result = await new SelectEntityActionPhase(address, actions);

                    if(actions.Contains(result.value))
                        return result.value;
                }

                return null;
            }
        }

        private SelectEntityActionPhase(EntityAddress entityAddress, List<IEntityAction> actions)
        {
            this.entityAddress = entityAddress;
            this.actions = actions;
        }

        public bool IsEmpty() => actions.Count == 0;

        public IEnumerable<T> GetAll<T>() where T : IEntityAction
        {
            foreach (IEntityAction iAction in actions)
            {
                if (iAction is T t)
                    yield return t;
            }
        }

        public bool Has<T>(out T action) where T : IEntityAction
        {
            foreach (IEntityAction iAction in actions)
            {
                if (iAction is T t)
                {
                    action = t;
                    return true;
                }
            }
            action = default;
            return false;
        }
    }
}