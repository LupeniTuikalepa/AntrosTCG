using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;

namespace ATCG.Battle.CapacitySystem.Status.Frost.Hardening
{
    public partial class HardeningStatus : Status<HardeningData,HardeningComponent,StatusDurationController>
    {
	    protected override HardeningComponent CreateStatusComponent(HardeningData data, in StatusContext context)
	    {
		    return new HardeningComponent(data, ChannelKey.GetUniqueChannelKey("HardeningStatus"));
		    
	    }

	    protected override StatusDurationController CreateStatusController(HardeningData data, in StatusContext context)
	    {
		    return new StatusDurationController(data.Duration);
	    }

	    protected override void OnApply(HardeningData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnApply(data, in statusInfos, in context);
		    EntityAddress target = statusInfos.targetAddress;
		    ChannelKey channelKey = statusInfos.StatusComponent.channelKey;
		    
		    if (target.TryGetComponentRO(out DefenseComponent defenseComponent))
			    defenseComponent.defense.Multiply(channelKey, data.DefenseBuff);
	    }

	    protected override void OnRemove(HardeningData data, in EntityStatusInfos statusInfos, in StatusContext context)
	    {
		    base.OnRemove(data, in statusInfos, in context);
		    EntityAddress target = statusInfos.targetAddress;
		    ChannelKey channelKey = statusInfos.StatusComponent.channelKey;
		    
		    if (target.TryGetComponentRO(out DefenseComponent defenseComponent))
			    defenseComponent.defense.RemoveOperation(channelKey);
	    }
    }
}
