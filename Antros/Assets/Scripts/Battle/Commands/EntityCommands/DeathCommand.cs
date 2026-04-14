using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class DeathCommand : EntityCommand
    {
        public DeathCommand(Entity targetEntity) : base(targetEntity)
        {
        }

        public override void Process(in GameCommandContext context)
        {
            EntityAddress address = TargetEntityAddress(context.World);
            if (address.ToAspect(out HeroEntityAspect aspect))
            {
                IBattlePlayer player = aspect.Player;
                player.AddOrRemoveHealth(-aspect.HeroCard.DeathCost);
            }

            address.Destroy();
        }
    }
}