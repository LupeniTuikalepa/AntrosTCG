using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Capacities.Setup;
using ATCG.Battle.CapacitySystem.Core.Setup.SelectCapacities;
using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local;
using ATCG.Capacities;
using ATCG.HexGrids;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle
{
	public class ChooseCapacityAction : EntityAction
	{
		private readonly HexCoordinates from;
		private readonly IEnumerable<CapacityData> capacities;
		public override int ManaCost => 0;
		
		public ChooseCapacityAction(LocalBattlePlayer fromPlayer,HexCoordinates from, IEnumerable<CapacityData> capacities) : base(fromPlayer)
		{
			this.from = from;
			this.capacities = capacities;
		}

		public  override async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
		{
			SelectCapacitiesPhase phase = new SelectCapacitiesPhase(fromPlayer, capacities, fromPlayer.CurrentMana);
			PhaseResult<CapacityData> result = await phase.Run();

			if (result.value != null)
			{
				CastCapacityAction capacityAction = new CastCapacityAction(fromPlayer, result.value, from);

				await capacityAction.Execute(address, battlePhase);
			}
		}
	}
}