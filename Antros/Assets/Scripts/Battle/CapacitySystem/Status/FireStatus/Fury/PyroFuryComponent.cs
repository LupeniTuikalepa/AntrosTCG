using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;
using Helteix.ChanneledProperties;

namespace ATCG.Battle.CapacitySystem.Status.FireStatus.Fury
{
	public readonly struct PyroFuryComponent : IStatusComponent
	{
		public readonly PyroFuryData data;
		public StatusData StatusData => data;
		public readonly int calculatedAttackBuff;
		public readonly ChannelKey channelKey;

		public PyroFuryComponent(PyroFuryData data, int calculatedAttackBuff, ChannelKey channelKey)
		{
			this.data = data;
			this.calculatedAttackBuff = calculatedAttackBuff;
			this.channelKey = channelKey;
		}

	}
}