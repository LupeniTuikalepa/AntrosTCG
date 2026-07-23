using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle
{
    public readonly struct HardeningComponent : IStatusComponent
    {
	    public readonly ChannelKey channelKey;
	    StatusData IStatusComponent.StatusData => data;
	    private readonly HardeningData data;

	    public HardeningComponent(HardeningData data, ChannelKey channelKey)
	    {
		    this.data = data;
		    this.channelKey = channelKey;
	    }
    }
}
