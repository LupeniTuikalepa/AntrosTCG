using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.GameModes;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Berserk
{
	public readonly struct BerserkStatusComponent : IStatusComponent
	{
		private readonly BerserkStatusData data;
		private readonly ChannelKey channelKey;
		public int CurrentDuration => data.Duration;
		StatusData IStatusComponent.StatusData => data;
		public ChannelKey ChannelKey => channelKey;
		
		public BerserkStatusComponent(BerserkStatusData data, ChannelKey channelKey)
		{
			this.data = data;
			this.channelKey = channelKey;
		}

		public void Trigger(EntityAddress address, BattlePhase battlePhase)
		{
		}
	}
}