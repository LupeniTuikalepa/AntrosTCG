using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Entities.Runtime.Deployables;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Constructions
{
    public class RuntimeConstructionSpawner : MonoBehaviour, ICommandListener<SpawnConstructionCommand>
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

        public async Awaitable Play(CommandListenerState state, CommandContext context, SpawnConstructionCommand command)
        {
            state.CompleteWindUp(this);

            var infos = command.GetInfos();

            GameObject instance = infos.prefab.InstantiatePrefab(transform);

            if (instance.TryGetComponent(out RuntimeConstruction runtimeConstruction))
                await runtimeConstruction.Spawn(runtimeEntityManager, infos.construction);


            state.CompleteFollowThrough(this);
        }
    }
}