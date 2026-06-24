using ATCG.Battle.Entities.Components.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Components.Implementations
{
    public readonly struct PoisonStatus : IStatus
    {
        private readonly int amount;

        public PoisonStatus(int amount)
        {
            this.amount = amount;
        }

        public void Trigger(EntityAddress address)
        {
            if (address.TryGetComponent<HealthComponent>(out var componentRef))
            {
                ref var component = ref componentRef.GetValue();
                component.AddOrRemoveHealth(-amount);
                Debug.Log($"[PoisonStatus] Triggered {amount} health points.");
            }
        }
    }
}