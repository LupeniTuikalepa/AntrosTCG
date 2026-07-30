using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Players;
using ATCG.Debugging.Cheats;
using UnityEngine.Pool;

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

		/// <summary>
		/// Enumerates every entity in the battle carrying component <typeparamref name="T"/> as a
		/// (label, address) target option — the natural source for a [CheatTarget] dropdown. The
		/// label is the card title when available, else the entity id.
		/// </summary>
		public static IEnumerable<CheatTargetOption> EnumerateTargets<T>(IBattlePlayer player)
			where T : struct, IEntityComponent
		{
			List<CheatTargetOption> list = new List<CheatTargetOption>();

			foreach (ComponentRef<T> componentRef in player.BattlePhase.world.Query<T>())
			{
				EntityAddress address = componentRef.EntityAddress;
				string label = address.TryGetComponentRO(out BattleCardComponent battleCardComponent)
					? battleCardComponent.battleCard.Title
					: componentRef.entityID.ToString();

				list.Add(new CheatTargetOption(label, address));
			}

			return list;
		}
	}
}