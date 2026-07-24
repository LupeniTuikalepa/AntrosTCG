using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;

namespace ATCG.Battle.CapacitySystem.Status.Frost.SlowDown
{
    public partial class SlowDownStatus : Status<SlowDownData, StatusDurationController>
    {
	    
	    private readonly ChannelKey channelKey = ChannelKey.GetUniqueChannelKey(nameof(SlowDownStatus));

	    protected override StatusDurationController CreateStatusController(SlowDownData data, in StatusContext context)
	    {
		    return new StatusDurationController(data.NormalDuration);

	    }


	    protected override bool Accepts(ComponentRef<StatusReceiver> componentRef)
	    {
		    return componentRef.EntityAddress.HasComponents<MovementComponent>();
	    }

	    protected override void OnStack(SlowDownData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnStack(data, in statusInfos, in context);

		    int total = statusInfos.StatusController.RemainingTicks + 1;
		    if (total >= data.MaxStack)
		    {
			    statusInfos.StatusController.SetTicks(total);
			    UpdateSlowAmount(data, statusInfos);
		    }
	    }

	    protected override void OnApply(SlowDownData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnApply(data, in statusInfos, in context);
		    
		    if (!statusInfos.targetAddress.TryGetComponent<MovementComponent>(out var componentRef))
			    return;
		    
		    componentRef.GetValue().moveSpeed.Subtract(channelKey, 0);
		    UpdateSlowAmount(data, statusInfos);

	    }

	    protected override void OnRemove(SlowDownData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnRemove(data, in statusInfos, in context);

		    if (!statusInfos.targetAddress.TryGetComponent<MovementComponent>(out var componentRef))
			    return;
		    
		    componentRef.GetValue().moveSpeed.RemoveOperation(channelKey);
	    }

	    private void UpdateSlowAmount(SlowDownData data, in EntityStatusInfos statusInfos)
	    {
		    if (!statusInfos.targetAddress.TryGetComponent<MovementComponent>(out var componentRef))
			    return;

		    ref var movementComponent = ref componentRef.GetValue();
		    int slow = data.Slow * statusInfos.StatusController.RemainingTicks;

		    movementComponent.moveSpeed.Write(channelKey, slow);
	    }
    }
}
