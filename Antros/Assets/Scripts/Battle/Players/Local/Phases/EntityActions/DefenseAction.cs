using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local;
using UnityEngine;

namespace ATCG.Battle
{
	public class DefenseAction : EntityAction
	{
		public override int ManaCost { get; }
		private readonly int defense;
		public DefenseAction(LocalBattlePlayer fromPlayer, int defense) : base(fromPlayer)
		{
			this.defense = defense;
		}

		public override async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
		{
			int finalDamage = this.defense;

			if (address.TryGetComponentRO<DefenseComponent>(out var defense))
			{
				finalDamage = Mathf.Max(0, finalDamage - defense.TotalDefense);
			}

			if (address.TryGetComponentRO<HealthComponent>(out var health))
			{
				health.AddOrRemoveHealth(-finalDamage);
			}
		}
	}
}