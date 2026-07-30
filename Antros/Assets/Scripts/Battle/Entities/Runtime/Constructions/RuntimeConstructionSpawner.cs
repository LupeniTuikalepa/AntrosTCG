using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime.Deployables;
using ATCG.Battle.PassiveSystem.Core;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Constructions
{
    public class RuntimeConstructionSpawner : MonoBehaviour, ICommandDirector<SpawnConstructionCommand>
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

        public async Awaitable Play(CommandDirectorState state, CommandContext context, SpawnConstructionCommand command)
        {
            state.CompleteWindUp(this);

            var infos = command.GetInfos();

            GameObject instance = infos.prefab.InstantiatePrefab(transform);

            var constructionAspect = infos.construction;
            if (instance.TryGetComponent(out RuntimeConstruction runtimeConstruction))
                await runtimeConstruction.Spawn(runtimeEntityManager, constructionAspect);

            state.CompleteFollowThrough(this);
        }
    }
}