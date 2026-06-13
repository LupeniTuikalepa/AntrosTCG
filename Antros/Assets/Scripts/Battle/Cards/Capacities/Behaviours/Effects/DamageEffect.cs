using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Effects;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Effects
{
    public class DamageEffect : ICapacityEffect<DamageEffectData>
    {
        public void Apply(DamageEffectData data, EntityAddress target,
            in CastCapacityCommand.Context context)
        {
            if (!target.HasComponent<HealthComponent>())
                return;

            DamageCommand damageCommand = new(data.Quantity, target);
            context.EmbedCommand(damageCommand);
        }
    }
}