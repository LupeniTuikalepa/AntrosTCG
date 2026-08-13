using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Capacities;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Setup.SelectCapacities
{
	public class SelectCapacitiesPhase : LocalPlayerPhaseCompletionSource<CapacityData>
	{
		public readonly IEnumerable<CapacityData> capacities;

		public SelectCapacitiesPhase(LocalBattlePlayer localBattlePlayer, IEnumerable<CapacityData> capacities ) : base(localBattlePlayer)
		{
			this.capacities = capacities;
		}

		protected override Awaitable<CapacityData> Execute(CancellationToken token)
		{
			if (!capacities.Any())
				return null;
			return base.Execute(token);
		}
	}
}