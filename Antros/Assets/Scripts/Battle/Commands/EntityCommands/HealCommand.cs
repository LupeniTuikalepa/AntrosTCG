using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class HealCommand : EntityCommand<DeltaInRangeInfos<int>>
    {

        public readonly int quantity;

        public HealCommand(int quantity, EntityAddress address) : base(address)
        {
            this.quantity = quantity;
        }

        protected override void Process(in CommandContext context)
        {
            if (!Target.TryGetComponent(context.World, out ComponentRef<HealthComponent> healthComponentRef))
                return;

            ref HealthComponent componentHealth = ref healthComponentRef.GetValue();
            infos.from = componentHealth.CurrentHealth;

            componentHealth.AddOrRemoveHealth(quantity);

            infos.to = componentHealth.CurrentHealth;
            infos.max = componentHealth.MaxHealth;
        }
    }
}