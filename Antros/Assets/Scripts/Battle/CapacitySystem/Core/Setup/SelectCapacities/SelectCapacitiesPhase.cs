using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Local.UI;
using ATCG.Capacities;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Setup.SelectCapacities
{
	public class SelectCapacitiesPhase : LocalPlayerPhaseCompletionSource<CapacityData>, ILocalHUDPhase<SelectCapacitiesPhase>
	{
		public readonly IEnumerable<CapacityData> capacities;
		private readonly int maxMana;

		public SelectCapacitiesPhase(LocalBattlePlayer localBattlePlayer, IEnumerable<CapacityData> capacities, int maxMana = 1) : base(localBattlePlayer)
		{
			this.capacities = capacities;
			this.maxMana = maxMana;
		}

		protected override Awaitable<CapacityData> Execute(CancellationToken token)
		{
			if (!capacities.Any(IsValid))
				return null;
			return base.Execute(token);
		}
		
		public bool IsValid(CapacityData capacityData) => maxMana < 0 || maxMana >= capacityData.Cost;
	}
}