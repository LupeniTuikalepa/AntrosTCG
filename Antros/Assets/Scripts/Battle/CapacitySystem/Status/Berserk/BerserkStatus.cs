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
			var channelKey = ChannelKey.GetUniqueChannelKey("Berserk");
			basicAttackerComponent.strength.Multiply(channelKey, data.forceMultiplier);
			
			if (target.TryGetComponentRO(out DefenseComponent defenseComponent))
				defenseComponent.defense.Subtract(channelKey, data.defenseReduction);

			target.ApplyStatus(new BerserkStatusComponent(data, channelKey),
				new StatusDurationController<BerserkStatusComponent>(data.Duration),
				context);
			
			Debug.Log($"Applying Berserk status: {target} : BuffAttack ==> {basicAttackerComponent.strength}  : DeBuffDefense ==> {defenseComponent.defense}");
		}

		public void Remove(BerserkStatusData data, EntityAddress address, StatusContext context)
		{
			if (address.TryGetComponentRO(out BerserkStatusComponent berserkComponent))
				berserkComponent.RemoveModifiers(address);

			address.RemoveStatus<BerserkStatusComponent>(context);
			
		}

		public void Tick(BerserkStatusData data, EntityAddress address, StatusContext context)
		{
			StatusManager.Trigger<BerserkStatusComponent>(address, context);
			Debug.Log($"[BerserkStatus] Tick]");
		}

		public void TickAll(BerserkStatusData data, StatusContext context)
		{
			StatusManager.ProcessAllStatus<BerserkStatusComponent>(context);
		}
	}
}