using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Berserk
{
	public partial struct BerserkStatus : IStatus<BerserkStatusData>
	{
		public void Apply(BerserkStatusData data, EntityAddress target, StatusContext context)
		{
			Debug.Log($"Applying Berserk status: {target}");
			
			if(!target.TryGetComponentRO(out BasicAttackerComponent  basicAttackerComponent))
				return;
			
			if(target.TryGetComponentRO(out DefenseComponent defenseComponent))
				defenseComponent.AddModifier(new DefenseModifier{ value = data.defenseDivision , sourceDescription = "Berserk" });
			
			var channelKey = ChannelKey.GetUniqueChannelKey("Berserk");
			basicAttackerComponent.strength.Multiply(channelKey, data.forceMultiplier);
			
			target.ApplyStatus(new BerserkStatusComponent(data, channelKey),
				new StatusDurationController<BerserkStatusComponent>(data.Duration),
				context);
		}

		public void Remove(BerserkStatusData data, EntityAddress address, StatusContext context)
		{
			if (address.TryGetComponentRO(out BerserkStatusComponent berserkComponent) && address.TryGetComponentRO(out BasicAttackerComponent attackerComponent))
			{
				attackerComponent.strength.RemoveOperation(berserkComponent.ChannelKey);
			}
			
			if(address.TryGetComponentRO(out DefenseComponent defenseComponent))
				defenseComponent.RemoveModifier(new DefenseModifier());
			
			address.RemoveStatus<BerserkStatusComponent>(context);
		}

		public void Tick(BerserkStatusData data, EntityAddress address, StatusContext context)
		{
		}

		public void TickAll(BerserkStatusData data, StatusContext context)
		{
		}
	}
}