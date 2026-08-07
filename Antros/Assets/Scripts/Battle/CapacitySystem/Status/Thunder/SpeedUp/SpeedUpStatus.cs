using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Status;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.SpeedUp
{
	
	public partial class SpeedUpStatus : Status<SpeedUpStatusData, StatusDurationController>
	{
		 private readonly ChannelKey channelKey = ChannelKey.GetUniqueChannelKey(nameof(SpeedUpStatus));

	    protected override StatusDurationController CreateStatusController(SpeedUpStatusData statusData, in StatusContext context)
	    {
		    return new StatusDurationController(statusData.NormalDuration);

	    }

	    protected override bool Accepts(ComponentRef<StatusReceiver> componentRef)
	    {
		    return componentRef.EntityAddress.HasComponents<MovementComponent>();
	    }

	    protected override void OnStack(SpeedUpStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnStack(statusData, in statusInfos, in context);
		    int total = statusInfos.StatusController.RemainingTicks + 1;

		    if (total >= statusData.MaxStack)
		    {
			    Debug.Log($"Max stack surpassé : {total}");
			    return;
		    }
		    
		    UpdateSpeedAmount(statusData, statusInfos);
		    statusInfos.StatusController.SetTicks(total);
		    return;
		    
	    }

	    protected override void OnApply(SpeedUpStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnApply(statusData, in statusInfos, in context);
		    if (!statusInfos.targetAddress.TryGetComponent<MovementComponent>(out var componentRef))
			    return;
		    
		    UpdateSpeedAmount(statusData, statusInfos);
		    componentRef.GetValue().moveSpeed.Add(channelKey,0);

	    }

	    protected override void OnRemove(SpeedUpStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnRemove(statusData, in statusInfos, in context);

		    if (!statusInfos.targetAddress.TryGetComponent<MovementComponent>(out var componentRef))
			    return;
		    componentRef.GetValue().moveSpeed.RemoveOperation(channelKey);
	    }

	    private void UpdateSpeedAmount(SpeedUpStatusData statusData, in EntityStatusInfos statusInfos)
	    {
		    if (!statusInfos.targetAddress.TryGetComponent<MovementComponent>(out var componentRef))
			    return;
			
		    ref var movementComponent = ref componentRef.GetValue();
		    int speedUp = statusData.SpeedUp * statusInfos.StatusController.RemainingTicks;
			
		    Debug.Log(speedUp);
		    movementComponent.moveSpeed.Write(channelKey, speedUp);
		    
	    }
	}
}