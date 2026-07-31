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

			// Guard the no-context case (e.g. a preview cheat built with no live player): no targets.
			if (player?.BattlePhase == null)
				return list;

			foreach (ComponentRef<T> componentRef in player.BattlePhase.world.Query<T>())
			{
				EntityAddress address = componentRef.EntityAddress;
				string title = address.TryGetComponentRO(out BattleCardComponent battleCardComponent)
					? battleCardComponent.battleCard?.Title
					: null;
				// Always suffix the entity id so labels are unique (distinct heroes/constructions
				// never collapse into one another) and never blank.
				string label = string.IsNullOrEmpty(title)
					? $"Missing Title #{componentRef.entityID}"
					: $"{title} #{componentRef.entityID}";

				list.Add(new CheatTargetOption(label, address));
			}

			return list;
		}
	}
}