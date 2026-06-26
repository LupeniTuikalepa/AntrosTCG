using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;

namespace ATCG.Debugging.Debugging.Battle
{
	public static class  CheatUtilities
	{
		public static void FillBucket<T>(Dictionary<string, EntityAddress> bucket, IBattlePlayer player) where T : struct, IEntityComponent
		{
			foreach (ComponentRef<T> componentRef in player.BattlePhase.world.Query<T>())
			{
				if (componentRef.EntityAddress.TryGetComponentRO(out BattleCardComponent battleCardComponent))
				{
					bucket.Add(battleCardComponent.battleCard.Title, componentRef.EntityAddress);
				}
				else
				{
					bucket.Add(componentRef.entityID.ToString(), componentRef.EntityAddress);
				}
			}
		}
	}
}