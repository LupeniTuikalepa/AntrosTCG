using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Utilities;
using UnityEngine;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class DamageCommand : EntityCommand<DeltaInRangeInfos<int>>
    {
        public readonly int quantity;

        public DamageCommand(int quantity, EntityAddress address, string source = DEFAULT_SOURCE) : base(address, source)
        {
            this.quantity = quantity;
        }

        protected override void Process(in CommandContext context)
        {
            EntityAddress address = TargetEntityAddress(context.World);

            if (!address.TryGetComponent(out ComponentRef<HealthComponent> healthComponentRef))
                return;
            int finalDamage = quantity;

            if (address.TryGetComponentRO(out DefenseComponent defenseComponent))
            {
	            int defenseValue = Mathf.Max(1, defenseComponent.Defense);
                float mult = (100f / (100f + defenseValue));
                finalDamage = GameMaths.Round(Mathf.Max(1, finalDamage * mult));

                Debug.Log($"Damage: from {quantity} to {finalDamage} for a defense of {defenseValue}");
            }

            ref HealthComponent componentHealth = ref healthComponentRef.GetValue();

            infos.from = componentHealth.CurrentHealth;
            componentHealth.AddOrRemoveHealth(-finalDamage);

            infos.to = componentHealth.CurrentHealth;
            infos.max = componentHealth.MaxHealth;

            if (componentHealth.CurrentHealth <= 0)
                Inject(context, new DeathCommand(address));
        }
    }
}