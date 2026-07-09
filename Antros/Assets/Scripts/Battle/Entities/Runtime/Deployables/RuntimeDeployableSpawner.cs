using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime.Heroes;
using ATCG.Metrics;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Deployables
{
    public class RuntimeDeployableSpawner : MonoBehaviour, ICommandListener<SpawnDeployableCommand>
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

        public async Awaitable Play(CommandListenerState state, CommandContext context, SpawnDeployableCommand command)
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