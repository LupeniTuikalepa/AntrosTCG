using System;
using System.Collections;
using System.Threading.Tasks;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Commands.Players;
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
            this.RegisterPlayer();
        }

        private void OnDisable()
        {
            this.UnregisterPlayer();
        }
        
        public async Awaitable Play(CommandPlayerState state, CommandContext context, SpawnHeroCommand command)
        {
            SpawnHeroCommand.Infos infos = command.GetInfos();

            EntityAddress address = infos.spawnedEntity.ToAddress(context.World);
            if (HeroEntityAspect.TryGetAspect(address, out HeroEntityAspect entityAspect))
            {
                GameObject instance = GameAssets.Current.HeroPawnPrefab.InstantiatePrefab(transform);

                if (instance.TryGetComponent(out RuntimeHero runtimeHeroBattleCard))
                    await runtimeHeroBattleCard.Spawn(runtimeEntityManager, entityAspect);
            }
        }
    }
}