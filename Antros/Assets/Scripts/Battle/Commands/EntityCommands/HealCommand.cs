using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class HealCommand : EntityCommand
    {
        public readonly int quantity;

        public HealCommand(int quantity, Entity entity) : base(entity)
        {
            this.quantity = quantity;
        }

        protected override void Process(in GameCommandContext context)
        {
            if (!TargetEntity.TryGetComponent(context.World, out ComponentRef<HealthComponent> healthComponentRef))
                return;

            ref HealthComponent componentHealth = ref healthComponentRef.GetValue();
            componentHealth.AddOrRemoveHealth(quantity);
        }
    }
}