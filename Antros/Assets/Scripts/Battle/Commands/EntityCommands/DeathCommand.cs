using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Players;

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

            if (address.Is(out HeroEntityAspect aspect))
            {
                IBattlePlayer player = aspect.Player;
	            player.AddOrRemoveHealth(-aspect.HeroCard.DeathCost);
	            
	            //Todo à changer 
	            player.DeadCards.TryAddCard(aspect.HeroCard);
            }
            address.Destroy();
            Break("Entity death.");
        }
    }
}