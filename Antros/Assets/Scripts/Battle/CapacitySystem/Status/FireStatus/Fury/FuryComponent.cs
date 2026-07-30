using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;
using Helteix.ChanneledProperties;

namespace ATCG.Battle.CapacitySystem.Status.Fury
{
	public readonly struct FuryComponent : IStatusComponent
	{
		public readonly PyroFuryData statusData;
		public StatusData StatusStatusData => statusData;
		public readonly ChannelKey channelKey;

		public FuryComponent(PyroFuryData statusData, ChannelKey channelKey)
		{
			this.statusData = statusData;
			this.channelKey = channelKey;
		}
	}
}