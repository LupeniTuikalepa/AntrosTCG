using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Components.Implementations
{
    public partial class PoisonStatus: Status<PoisonStatusData, StatusDurationController>
    {
        protected override StatusDurationController CreateStatusController(PoisonStatusData data, in StatusContext context)
        {
            return new StatusDurationController(data.Duration);
        }

        protected override void OnTick(PoisonStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
        {
            DamageCommand damageCommand = new DamageCommand(data.Damage, statusInfos.targetAddress);
            damageCommand.Run(context.battlePhase);

            base.OnTick(data, in statusInfos, context);
        }

        protected override void OnStack(PoisonStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
        {
            statusInfos.StatusController.AddOrRemoveTicks(data.Duration);

            base.OnStack(data, in statusInfos, in context);
        }
    }
}