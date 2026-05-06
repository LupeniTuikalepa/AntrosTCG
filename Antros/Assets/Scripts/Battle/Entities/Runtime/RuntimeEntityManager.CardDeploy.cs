using ATCG.Battle.Cards;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.GameCommands;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime
{
    public partial class RuntimeEntityManager : ICommandPlayer<DeployCardCommand>
    {
        async Awaitable ICommandPlayer<DeployCardCommand>.Play(DeployCardCommand command)
        {
            /*
            EntityAddress deployedEntity = command.DeployedEntity;
            if (deployedEntity.TryConvertToAspect(out HeroEntityAspect heroEntityAspect))
            {
                if (heroCards.ContainsKey(heroBattleCard))
                    break;

                GameObject instance = GameAssets.Current.HeroPawnPrefab.InstantiatePrefab(container);
                if (instance.TryGetComponent(out RuntimeHero runtimeHeroBattleCard))
                {
                    runtimeHeroBattleCard.Initialize(this);
                    //runtimeHeroBattleCard.Connect(HeroCard);

                    heroCards.Add(heroBattleCard, runtimeHeroBattleCard);
                }
            }
            */
        }


        private void OnCardDeployed(IBattleCard card)
        {
            switch (card)
            {
                case HeroBattleCard heroBattleCard:


                    break;
            }
        }

    }
}