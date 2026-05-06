using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class DamageCommand : EntityCommand
    {
        public readonly int quantity;
        public readonly bool triggerDealDamageReactions;

        public DamageCommand(int quantity, Entity entity, bool triggerDealDamageReactions = true) :
            base(entity)
        {
            this.quantity = quantity;
            this.triggerDealDamageReactions = triggerDealDamageReactions;
        }

        protected override void Process(in GameCommandContext context)
        {
            EntityAddress address = TargetEntityAddress(context.World);

            if (!address.TryGetComponent(out ComponentRef<HealthComponent> healthComponentRef))
                return;

            ref HealthComponent componentHealth = ref healthComponentRef.GetValue();
            componentHealth.AddOrRemoveHealth(-quantity);
            if (componentHealth.CurrentHealth <= 0)
                Embed(context, new DeathCommand(TargetEntity));
        }
    }
}