using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class HealCommand : EntityCommand<HealCommand.Infos>
    {
        public struct Infos
        {
            public int fromHealth;
            public int toHealth;
        }

        public readonly int quantity;

        public HealCommand(int quantity, Entity entity) : base(entity)
        {
            this.quantity = quantity;
        }

        protected override void Process(in CommandContext context)
        {
            if (!Target.TryGetComponent(context.World, out ComponentRef<HealthComponent> healthComponentRef))
                return;

            ref HealthComponent componentHealth = ref healthComponentRef.GetValue();
            infos.fromHealth = componentHealth.CurrentHealth;

            componentHealth.AddOrRemoveHealth(quantity);

            infos.toHealth = componentHealth.CurrentHealth;
        }
    }
}