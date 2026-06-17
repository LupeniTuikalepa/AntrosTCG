using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class DamageCommand : EntityCommand<NoInfos>
    {

        public readonly int quantity;
        public readonly bool triggerDealDamageReactions;

        public DamageCommand(int quantity, EntityAddress address, bool triggerDealDamageReactions = true) : base(address)
        {
            this.quantity = quantity;
            this.triggerDealDamageReactions = triggerDealDamageReactions;
        }

        protected override void Process(in CommandContext context)
        {
            EntityAddress address = TargetEntityAddress(context.World);

            if (!address.TryGetComponent(out ComponentRef<HealthComponent> healthComponentRef))
                return;

            ref HealthComponent componentHealth = ref healthComponentRef.GetValue();
            
            infos.lastHealth = componentHealth.CurrentHealth;
            componentHealth.AddOrRemoveHealth(-quantity);
            infos.currentHealth = componentHealth.CurrentHealth;
            infos.maxHealth = componentHealth.MaxHealth;
            
            if (componentHealth.CurrentHealth <= 0)
                Embed(context, new DeathCommand(address));
        }
    }
}