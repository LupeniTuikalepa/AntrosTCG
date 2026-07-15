using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.CapacitySystem.Status.Berserk;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct FightMadness : ICapacity<FightMadnessData>
	{
		public HexPatternBuilder GetHitPattern(FightMadnessData data, BattleGrid battleGrid, HexCoordinates castPoint,
			HexCoordinates casterOrigin)
		{
			BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
			HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
				.With(new PointsPattern(castPoint));

			return builder;
		}

		private partial void ExecuteDeployRage(FightMadnessData data, CapacityStepContext ctx)
		{
			Debug.Log($"DeployRage,{data.BerserkData}", data);

			// Must go through StatusApplyCommand (not IStatusContainer.Apply directly):
			// running it is what dispatches to RuntimeEntity's StatusApplyCommand listener,
			// which instantiates the status's RuntimeStatus prefab and fires
			// IRuntimeStatusComponent.OnApplyStatus — i.e. the VFX. Calling container.Apply
			// straight away only does the ECS-side status/controller/signature bookkeeping,
			// with nothing spawning the visual side.
			var statusCommand = new StatusApplyCommand(ctx.Caster, data.BerserkData);
			statusCommand.Run(ctx.BattlePhase);
		}

		private partial void ExecutePunch(FightMadnessData data, CapacityStepContext ctx)
		{
			if (ctx.BattleGrid.TryGetBattleCell(ctx.CastPoint, out var cell))
			{
				foreach (var componentRef in cell.GetMembers())
				{
					if (componentRef.EntityAddress.HasComponent<HealthComponent>())
					{
						var damage = new DamageCommand( data.PunchDamage, componentRef.EntityAddress);
						damage.Run(ctx.BattlePhase);
					}
				}
			}
		}
	}
}