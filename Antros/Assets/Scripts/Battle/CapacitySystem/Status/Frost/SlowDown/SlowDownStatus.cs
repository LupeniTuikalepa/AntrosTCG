using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle
{
    public partial class SlowDownStatus : Status<SlowDownData,SlowDownComponent,StatusDurationController>
    {
	    protected override SlowDownComponent CreateStatusComponent(SlowDownData data, in StatusContext context)
	    {
		    SlowDownComponent slowDownComponent = new SlowDownComponent(data, ChannelKey.GetUniqueChannelKey("SlowDown"));
		    return slowDownComponent;
	    }

	    protected override StatusDurationController CreateStatusController(SlowDownData data, in StatusContext context)
	    {
		    return new StatusDurationController(data.NormalDuration);

	    }

	    protected override void OnStack(SlowDownData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    statusInfos.StatusController.AddOrRemoveTicks(data.AddDuration);
		    base.OnStack(data, in statusInfos, in context);
	    }

	    protected override void OnApply(SlowDownData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnApply(data, in statusInfos, in context);
		    
		    int slow = data.Slow * statusInfos.StatusController.RemainingTicks;
		    ChannelKey channelKey = statusInfos.StatusComponent.key;
		    if (statusInfos.targetAddress.HasComponent<MovementComponent>())
		    {
			    var movementComponent = statusInfos.targetAddress.GetComponent<MovementComponent>();
			    movementComponent.moveSpeed.Subtract(channelKey , slow);
		    }

	    }
	    protected override void OnRemove(SlowDownData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    ChannelKey channelKey = statusInfos.StatusComponent.key;
		    if (statusInfos.targetAddress.HasComponent<MovementComponent>())
		    {
			    var movementComponent = statusInfos.targetAddress.GetComponent<MovementComponent>();
			    movementComponent.moveSpeed.RemoveOperation(channelKey);
		    }
		    base.OnRemove(data, in statusInfos, in context);
	    }
    }
}
