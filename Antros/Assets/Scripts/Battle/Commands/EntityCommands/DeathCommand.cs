using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
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

            if (address.Is(out HeroEntityAspect aspect))
            {
                IBattlePlayer player = aspect.Player;
                
                Embed(in context, new ModifyPlayerHealthCommand(player, -aspect.HeroCard.DeathCost));
                
	            //Todo à changer 
	            player.DeadCards.TryAddCard(aspect.HeroCard);
	            
            }
            address.Destroy();
            //Break("Entity death.");
        }
    }
}