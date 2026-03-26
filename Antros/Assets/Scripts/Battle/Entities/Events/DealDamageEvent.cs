using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Core;
using ATCG.Battle.Timelines;

namespace ATCG.Battle.Entities.Events
{
    public class DealDamageEvent : EntityEvent
    {
        public readonly int quantity;
        public readonly bool triggerDealDamageReactions;

        public DealDamageEvent(int quantity, 
            bool triggerDealDamageReactions = true,
            EntityEvent parent = null, 
            params EntityAddress[] entities) : base(parent, entities)
        {
            this.quantity = quantity;
            this.triggerDealDamageReactions = triggerDealDamageReactions;
        }
        
        public override void Apply()
        {
            for (int i = 0; i < TargetedEntities.Length; i++)
            {
                var address = TargetedEntities[i];
                var entity = address.entity;
                var world = address.world;
                
                if (entity.TryGetComponent<HealthComponent>(world, out var healthComponentRef))
                {
                    ref HealthComponent componentHealth = ref healthComponentRef.GetValue();
                    componentHealth.AddOrRemoveHealth(-quantity);

                    if (componentHealth.currentHealth <= 0)
                    {
                        DeathEvent deathEvent = new DeathEvent(this, address);
                        deathEvent.Apply();
                        return;
                    }
                }
            }
        }
    }
}