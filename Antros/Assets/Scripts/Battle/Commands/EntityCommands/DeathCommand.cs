using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;
using ATCG.Battle.Players.UI;
using ATCG.UI;
using UnityEngine;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class DeathCommand : EntityCommand<NoInfos>
    {
        public DeathCommand(EntityAddress address) : base(address)
        {

        }

        protected override void Process(in CommandContext context)
        {
            EntityAddress address = TargetEntityAddress(context.World);

            if (address.TryGetComponentRO(out DeathCostComponent deathCostComponent) &&
                address.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayerComponent))
            {
                IBattlePlayer player = belongsToPlayerComponent.GetPlayer(context.battlePhase);
                Inject(in context, new ModifyPlayerHealthCommand(player, -deathCostComponent.cost));
            }

            //TODO à changer
            if (address.Is(out HeroEntityAspect aspect))
                aspect.Player.DeadCards.TryAddCard(aspect.HeroCard);

            address.Destroy();
            //Break("Entity death.");
        }
    }
}