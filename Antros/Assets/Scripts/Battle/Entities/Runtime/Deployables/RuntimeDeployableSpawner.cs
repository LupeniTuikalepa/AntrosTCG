using ATCG.Battle.Commands;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.EntityCommands;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Deployables
{
    public class RuntimeDeployableSpawner : MonoBehaviour, ICommandDirector<SpawnDeployableCommand>
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

        public async Awaitable Play(CommandDirectorState state, CommandContext context, SpawnDeployableCommand command)
        {
            state.CompleteWindUp(this);

            var infos = command.GetInfos();

            GameObject instance = infos.data.Prefab.InstantiatePrefab(transform);

            if (instance.TryGetComponent(out RuntimeDeployable runtimeDeployable))
                await runtimeDeployable.Spawn(runtimeEntityManager, infos.deployable);


            state.CompleteFollowThrough(this);
        }
    }
}