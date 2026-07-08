using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Status;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Status.Berserk
{
	public static class BerserkStatusExtensions
	{
		private const int GuaranteedRemainingTicks = 2;

		public static void RefreshBerserk(this EntityAddress address)
		{
			if (!address.TryGetComponentRO(out StatusDurationController<BerserkStatusComponent> durationController))
				return;
			
			if (durationController.RemainingTicks >= GuaranteedRemainingTicks)
				return;
			
			address.AddOrSetComponent(new StatusDurationController<BerserkStatusComponent>(GuaranteedRemainingTicks));
			Debug.Log($"[BerserkStatusExtensions]: Refreshing ==> {address.entity.id} BerserkDuration ==> {GuaranteedRemainingTicks}");
		}
	}
}