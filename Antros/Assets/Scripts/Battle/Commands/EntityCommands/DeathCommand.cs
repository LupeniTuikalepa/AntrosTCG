using ATCG.Battle.Commands.Entities;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Commands.Infos;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.PassiveSystem.Core;
using ATCG.Battle.Players;
using UnityEngine;

namespace ATCG.Battle.Commands.EntityCommands
{
    public class DeathCommand : EntityCommand<NoInfos>
    {
        public const string NATURAL_DEATH = "NATURAL_DEATH";
        
        
        public DeathCommand(EntityAddress address, string source = NATURAL_DEATH) : base(address, source)
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

            if(address.TryGetComponentRO<PassiveContainerComponent>(out var passiveContainer))
                passiveContainer.RemoveAllPassive(address);
            
            address.Destroy();
            //Break("Entity death.");
        }
    }
}