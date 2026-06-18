using System;
using System.Collections;
using System.Threading.Tasks;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Metrics;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Heroes
{
    public class RuntimeHeroSpawner : MonoBehaviour, ICommandListener<SpawnHeroCommand>
    {
        [SerializeField]
        private RuntimeEntityManager runtimeEntityManager;

        private void OnEnable()
        {
            this.RegisterListener();
        }

        private void OnDisable()
        {
            this.UnregisterListener();
        }

        public async Awaitable Play(CommandListenerState state, CommandContext context, SpawnHeroCommand command)
        {
            state.CompleteWindUp(this);

            if (command.SpawnID.TryGetEntityWithBattleID(context.World, out EntityAddress entityAddress))
            {
                if (HeroEntityAspect.TryGetAspect(entityAddress, out HeroEntityAspect entityAspect))
                {
                    GameObject instance = GameAssets.Current.HeroPawnPrefab.InstantiatePrefab(transform);

                    if (instance.TryGetComponent(out RuntimeHero runtimeHeroBattleCard))
                        await runtimeHeroBattleCard.Spawn(runtimeEntityManager, entityAspect);
                }
            }

            state.CompleteFollowThrough(this);
        }
    }
}