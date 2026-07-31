using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;

namespace ATCG.Battle.CapacitySystem.Status.Frost.SlowDown
{
    public partial class SlowDownStatus : Status<SlowDownStatusData, StatusDurationController>
    {
	    
	    private readonly ChannelKey channelKey = ChannelKey.GetUniqueChannelKey(nameof(SlowDownStatus));

	    protected override StatusDurationController CreateStatusController(SlowDownStatusData statusData, in StatusContext context)
	    {
		    return new StatusDurationController(statusData.NormalDuration);

	    }


	    protected override bool Accepts(ComponentRef<StatusReceiver> componentRef)
	    {
		    return componentRef.EntityAddress.HasComponents<MovementComponent>();
	    }

	    protected override void OnStack(SlowDownStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnStack(statusData, in statusInfos, in context);

		    int total = statusInfos.StatusController.RemainingTicks + 1;
		    if (total >= statusData.MaxStack)
		    {
			    statusInfos.StatusController.SetTicks(total);
			    UpdateSlowAmount(statusData, statusInfos);
		    }
	    }

	    protected override void OnApply(SlowDownStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnApply(statusData, in statusInfos, in context);
		    
		    if (!statusInfos.targetAddress.TryGetComponent<MovementComponent>(out var componentRef))
			    return;
		    
		    componentRef.GetValue().moveSpeed.Subtract(channelKey, 0);
		    UpdateSlowAmount(statusData, statusInfos);

	    }

	    protected override void OnRemove(SlowDownStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnRemove(statusData, in statusInfos, in context);

		    if (!statusInfos.targetAddress.TryGetComponent<MovementComponent>(out var componentRef))
			    return;
		    
		    componentRef.GetValue().moveSpeed.RemoveOperation(channelKey);
	    }

	    private void UpdateSlowAmount(SlowDownStatusData statusData, in EntityStatusInfos statusInfos)
	    {
		    if (!statusInfos.targetAddress.TryGetComponent<MovementComponent>(out var componentRef))
			    return;

		    ref var movementComponent = ref componentRef.GetValue();
		    int slow = statusData.Slow * statusInfos.StatusController.RemainingTicks;

		    movementComponent.moveSpeed.Write(channelKey, slow);
	    }
    }
}
