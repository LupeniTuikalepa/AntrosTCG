using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle
{
    public readonly struct SlowDownComponent : IStatusComponent
    {
	    StatusData IStatusComponent.StatusData => data;
	    private readonly SlowDownData data;
	    public readonly ChannelKey key;

	    public SlowDownComponent(SlowDownData data, ChannelKey key)
	    {
		    this.data = data;
		    this.key = key;
	    }
    }
}
