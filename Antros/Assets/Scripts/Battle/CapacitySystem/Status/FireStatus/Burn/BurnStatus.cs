using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Iterations;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using UnityEngine;

namespace ATCG.Battle
{
    public partial class BurnStatus : Status<BurnStatusData, BurnStatusComponent, StatusDurationController>, ITickOnTurnBegin
    {
	    protected override BurnStatusComponent CreateStatusComponent(BurnStatusData data, in StatusContext context)
	    {
		    return new BurnStatusComponent(data);
	    }

	    protected override StatusDurationController CreateStatusController(BurnStatusData data, in StatusContext context)
	    {
		    return new StatusDurationController(data.normalDuration);
	    }


	    protected override void OnStack(BurnStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    statusInfos.StatusController.AddOrRemoveTicks(data.normalDuration);
		    base.OnStack(data, in statusInfos, in context);
	    }

	    protected override void OnTick(BurnStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnTick(data, in statusInfos, context);
		    int damage = data.Damage * statusInfos.StatusController.RemainingTicks;

		    Debug.Log(damage + " damage sur " + statusInfos.targetAddress);
		    if (statusInfos.targetAddress.HasComponent<HealthComponent>())
		    {
			    DamageCommand damageCommand = new DamageCommand(damage, statusInfos.targetAddress);
			    damageCommand.Run(context.battlePhase);
		    }
	    }
    }
}