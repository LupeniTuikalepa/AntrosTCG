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
			if (!address.TryGetComponentRO(out StatusDurationController<BerserkStatusComponent> statusDurationController))
				return;
			
			if (statusDurationController.RemainingTicks >= 1)
				return;
			Debug.Log("AAAAAAAAAA" + statusDurationController.RemainingTicks);
			
			if (address.TryGetComponentRO(out BasicAttackerComponent attackerComponent))
				attackerComponent.strength.RemoveOperation(channelKey);
			
			if (address.TryGetComponentRO(out DefenseComponent defenseComponent))
				defenseComponent.defense.RemoveOperation(channelKey);
			RemoveModifiers(address);
		}

		public void RemoveModifiers(EntityAddress address)
		{
			if (address.TryGetComponentRO(out BasicAttackerComponent attackerComponent))
				attackerComponent.strength.RemoveOperation(channelKey);
			
			if (address.TryGetComponentRO(out DefenseComponent defenseComponent))
				defenseComponent.defense.RemoveOperation(channelKey);
			Debug.Log($" Attack == {attackerComponent.strength} Defense == {defenseComponent.defense}");
		}
	}
}