using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Effects;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Effects
{
    public class HealEffect : ICapacityEffect<HealEffectData>
    {
        public void Apply(HealEffectData data, EntityAddress target,
            in CastCapacityCommand.Context context)
        {
            if (!target.HasComponent<HealthComponent>())
                return;

            HealCommand healCommand = new(data.Quantity, target);
            context.EmbedCommand(healCommand);
        }
    }
}