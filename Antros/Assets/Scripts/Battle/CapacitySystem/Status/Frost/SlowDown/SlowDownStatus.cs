using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;

namespace ATCG.Battle.CapacitySystem.Status.Frost.SlowDown
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
		    base.OnStack(data, in statusInfos, in context);
		    statusInfos.StatusController.AddOrRemoveTicks(data.AddDuration);
		    UpdateSlowAmount(data, statusInfos);
	    }

	    protected override void OnApply(SlowDownData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnApply(data, in statusInfos, in context);
		    
		    UpdateSlowAmount(data, statusInfos);

	    }
	    protected override void OnRemove(SlowDownData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnRemove(data, in statusInfos, in context);
		    ChannelKey channelKey = statusInfos.StatusComponent.key;
		    if (statusInfos.targetAddress.HasComponent<MovementComponent>())
		    {
			    var movementComponent = statusInfos.targetAddress.GetComponent<MovementComponent>();
			    movementComponent.moveSpeed.RemoveOperation(channelKey);
		    }
	    }
	    private void UpdateSlowAmount(SlowDownData data, in EntityStatusInfos statusInfos)
	    {
		    if (!statusInfos.targetAddress.HasComponent<MovementComponent>())
			    return;

		    var movementComponent = statusInfos.targetAddress.GetComponent<MovementComponent>();
		    ChannelKey channelKey = statusInfos.StatusComponent.key;
		    int slow = data.Slow * statusInfos.StatusController.RemainingTicks;

		    movementComponent.moveSpeed.RemoveOperation(channelKey);
		    
		    movementComponent.moveSpeed.Subtract(channelKey, slow);
	    }
    }
}
