using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;
using Helteix.ChanneledProperties;

namespace ATCG.Battle.CapacitySystem.Status.Fury
{
	public readonly struct FuryComponent : IStatusComponent
	{
		public readonly PyroFuryData data;
		public StatusData StatusData => data;
		public readonly ChannelKey channelKey;

		public FuryComponent(PyroFuryData data, ChannelKey channelKey)
		{
			this.data = data;
			this.channelKey = channelKey;
		}
	}
}