using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Frost.Hardening
{
    public partial class HardeningStatus : Status<HardeningData, StatusDurationController>
    {
	    private readonly ChannelKey channelKey = ChannelKey.GetUniqueChannelKey(nameof(HardeningStatus));

	    protected override StatusDurationController CreateStatusController(HardeningData data, in StatusContext context)
	    {
		    return new StatusDurationController(data.Duration);
	    }

	    protected override void OnApply(HardeningData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnApply(data, in statusInfos, in context);
		    EntityAddress target = statusInfos.targetAddress;
		    if (target.TryGetComponentRO(out DefenseComponent defenseComponent))
			    defenseComponent.defense.Add(channelKey, data.DefenseBuff);
		    
		    Debug.Log($"{statusInfos.statusAddress.entity.id} voit sa defense augmenter de {defenseComponent.defense}");
	    }

	    protected override void OnRemove(HardeningData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnRemove(data, in statusInfos, in context);
		    EntityAddress target = statusInfos.targetAddress;
		    if (target.TryGetComponentRO(out DefenseComponent defenseComponent))
			    defenseComponent.defense.RemoveOperation(channelKey);
	    }
    }
}
