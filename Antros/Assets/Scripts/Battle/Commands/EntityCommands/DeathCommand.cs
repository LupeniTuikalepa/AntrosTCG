using ATCG.Battle.Commands.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class DeathCommand : EntityCommand<DeathCommand.Infos>
    {
        public struct Infos
        {

        }

        public DeathCommand(Entity sourceEntity) : base(sourceEntity)
        {

        }

        protected override void Process(in CommandContext context)
        {
            EntityAddress address = TargetEntityAddress(context.World);

            if (address.IsHeroEntityAspect(out HeroEntityAspect aspect))
            {
                IBattlePlayer player = aspect.Player;
                player.AddOrRemoveHealth(-aspect.HeroCard.DeathCost);
            }

            address.Destroy();
            Break("Entity death.");
        }
    }
}