using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Iterations;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle
{
    public partial class FlameStatus : Status<FlameStatusData, FlameStatusComponent, StatusDurationController>,ITickOnTurnBegin
    {
	    protected override FlameStatusComponent CreateStatusComponent(FlameStatusData data, in StatusContext context)
	    {
		    return new FlameStatusComponent(data);
	    }

	    protected override StatusDurationController CreateStatusController(FlameStatusData data, in StatusContext context)
	    {
		    return new StatusDurationController(data.normalDuration);
	    }


	    protected override void OnStack(FlameStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    statusInfos.StatusController.AddOrRemoveTicks(data.normalDuration);
		    base.OnStack(data, in statusInfos, in context);
	    }

	    protected override void OnTick(FlameStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnTick(data, in statusInfos, context);
		    int damage = data.Damage * statusInfos.StatusController.RemainingTicks;

		    if (statusInfos.targetAddress.HasComponent<HealthComponent>())
		    {
			    DamageCommand damageCommand = new DamageCommand(damage, statusInfos.targetAddress);
			    damageCommand.Run(context.battlePhase);
		    }
	    }
    }
}