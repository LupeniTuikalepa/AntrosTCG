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
		private ChannelKey channelKey;
		
		public void Apply(BerserkStatusData data, EntityAddress target, StatusContext context)
		{
			if(!target.TryGetComponentRO(out BasicAttackerComponent  basicAttackerComponent))
				return;
			channelKey = ChannelKey.GetUniqueChannelKey("Berserk");
			basicAttackerComponent.strength.Multiply(channelKey, data.forceMultiplier);
			
			if (target.TryGetComponentRO(out DefenseComponent defenseComponent))
				defenseComponent.defense.Subtract(channelKey, data.defenseReduction);

			target.ApplyStatus(new BerserkStatusComponent(data, channelKey),
				new StatusVolatileController<BerserkStatusComponent>(),
				context);
		}

		public void Remove(BerserkStatusData data, EntityAddress address, StatusContext context)
		{ 
			
			if (address.TryGetComponentRO(out BasicAttackerComponent attackerComponent))
					attackerComponent.strength.RemoveOperation(channelKey);
			
			if (address.TryGetComponentRO(out DefenseComponent defenseComponent))
				defenseComponent.defense.RemoveOperation(channelKey);
			
			address.RemoveStatus<BerserkStatusComponent>(address,context);
			
		}

		public void Tick(BerserkStatusData data, EntityAddress address, StatusContext context)
		{
		}

		public void TickAll(BerserkStatusData data, StatusContext context)
		{
		}
	}
}