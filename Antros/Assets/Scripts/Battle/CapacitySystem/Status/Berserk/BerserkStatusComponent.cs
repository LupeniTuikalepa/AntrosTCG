using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.GameModes;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;

namespace ATCG.Battle.CapacitySystem.Status.Berserk
{
	public readonly struct BerserkStatusComponent : IStatusComponent
	{
		private readonly BerserkStatusData data;
		private readonly ChannelKey channelKey;
		StatusData IStatusComponent.StatusData => data;
		public ChannelKey ChannelKey => channelKey;
		
		public BerserkStatusComponent(BerserkStatusData data, ChannelKey channelKey)
		{
			this.data = data;
			this.channelKey = channelKey;
		}

		public void Trigger(EntityAddress address, BattlePhase battlePhase)
		{
			if (!address.TryGetComponentRO(out StatusDurationController<BerserkStatusComponent> statusDurationController))
				return;
			
			if (statusDurationController.RemainingTicks >= 1)
				return;
			
			if (address.TryGetComponentRO(out BasicAttackerComponent attackerComponent))
				attackerComponent.strength.RemoveOperation(channelKey);
				
		}
	}
}