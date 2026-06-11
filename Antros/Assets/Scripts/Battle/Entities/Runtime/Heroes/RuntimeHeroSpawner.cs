using System;
using System.Collections;
using System.Threading.Tasks;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Metrics;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Heroes
{
    public class RuntimeHeroSpawner : MonoBehaviour, ICommandPlayer<SpawnHeroCommand>
    {
        [SerializeField]
        private RuntimeEntityManager runtimeEntityManager;

        private void OnEnable()
        {
            GameCommandManager.Instance.Register(this);
        }

        private void OnDisable()
        {
            GameCommandManager.Instance.Unregister(this);
        }


        async Awaitable ICommandPlayer<SpawnHeroCommand>.Play(SpawnHeroCommand command)
        {
            if (HeroEntityAspect.TryGetAspect(command.DeployedEntity, out HeroEntityAspect entityAspect))
            {
                GameObject instance = GameAssets.Current.HeroPawnPrefab.InstantiatePrefab(transform);
                
                if (instance.TryGetComponent(out RuntimeHero runtimeHeroBattleCard))
                    await runtimeHeroBattleCard.Spawn(runtimeEntityManager, entityAspect);
            }
        }

    }
}