using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Capacities.Data.Status;
using Sirenix.OdinInspector;

namespace ATCG.Battle.Entities.Runtime
{
#if UNITY_EDITOR

    public partial class RuntimeEntity<T>
    {
        [Button, DisableInEditorMode]
        private void ApplyStatus(StatusData data)
        {
            var statusApplyCommand = new StatusApplyCommand(Address, data);
            statusApplyCommand.Run(BattlePhase);
        }
    }

#endif
}