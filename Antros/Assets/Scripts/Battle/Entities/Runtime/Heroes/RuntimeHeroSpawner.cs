using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.PassiveSystem.Core;
using ATCG.Cards.Implementations;
using ATCG.Metrics;
using ATCG.Passives.Datas;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Heroes
{
    public class RuntimeHeroSpawner : MonoBehaviour, ICommandDirector<SpawnHeroCommand>
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

        public async Awaitable Play(CommandDirectorState state, CommandContext context, SpawnHeroCommand command)
        {
            state.CompleteWindUp(this);

            if (command.SpawnID.TryGetEntityWithBattleID(context.World, out EntityAddress entityAddress))
            {
                if (HeroEntityAspect.TryGetAspect(entityAddress, out HeroEntityAspect entityAspect))
                {
	                var instance = GameAssets.Current.HeroPawnPrefab.InstantiatePrefab(transform);

	                 if (instance.TryGetComponent(out RuntimeHero runtimeHeroBattleCard))
	                     await runtimeHeroBattleCard.Spawn(runtimeEntityManager, entityAspect);
                }
            }

            state.CompleteFollowThrough(this);
        }
    }
}