using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.ChanneledProperties;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Berserk
{
	public partial class BerserkStatusBehaviour : StatusBehaviour<
		BerserkStatusData,
		BerserkStatusComponent,
		StatusVolatileController>
	{
		protected override BerserkStatusComponent CreateStatusComponent(BerserkStatusData data, in StatusContext context)
		{
			BerserkStatusComponent berserkStatusComponent = new BerserkStatusComponent(data, ChannelKey.GetUniqueChannelKey("Berserk"));
			return berserkStatusComponent;
		}

		protected override StatusVolatileController CreateStatusController(BerserkStatusData data, in StatusContext context) => new();

		protected override void OnApply(BerserkStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			EntityAddress target = statusInfos.targetAddress;

			//listens for entity events
			ref BerserkStatusComponent berserkStatusComponent = ref statusInfos.statusComponentRef.GetValue();
			berserkStatusComponent.Watch(statusInfos.targetAddress, statusInfos.statusControllerRef);

			//Debug.Log($"Applying Berserk status: {target}");
			if(!target.TryGetComponentRO(out BasicAttackerComponent  basicAttackerComponent))
				return;

			ChannelKey channelKey = statusInfos.StatusComponent.channelKey;
			basicAttackerComponent.strength.Multiply(channelKey, data.forceMultiplier);

			if (target.TryGetComponentRO(out DefenseComponent defenseComponent))
				defenseComponent.defense.Subtract(channelKey, data.defenseReduction);

			base.OnApply(data, in statusInfos, in context);
		}


		protected override void OnRemove(BerserkStatusData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			EntityAddress target = statusInfos.targetAddress;

			if(!target.TryGetComponentRO(out BasicAttackerComponent  basicAttackerComponent))
				return;

			ChannelKey channelKey = statusInfos.StatusComponent.channelKey;
			basicAttackerComponent.strength.RemoveOperation(channelKey);

			if (target.TryGetComponentRO(out DefenseComponent defenseComponent))
				defenseComponent.defense.RemoveOperation(channelKey);

			base.OnRemove(data, in statusInfos, in context);
		}
	}
}