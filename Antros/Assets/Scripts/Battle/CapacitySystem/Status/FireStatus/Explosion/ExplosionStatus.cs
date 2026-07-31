using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Status.Controllers;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Data.Status;
using ATCG.Capacities.Status.FireStatus;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Status.Explosion
{
	public partial class ExplosionStatus : Status<ExplosionStatusData,StatusDurationController>
	{
		protected override StatusDurationController CreateStatusController(ExplosionStatusData statusData, in StatusContext context)
		{
			return new StatusDurationController(statusData.Duration);
		}

		protected override void OnStack(ExplosionStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnStack(statusData, in statusInfos, in context);
			statusInfos.StatusController.AddOrRemoveTicks(statusData.AddStack);
			base.OnStack(statusData, in statusInfos, in context);
		}

		protected override void OnTick(ExplosionStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnTick(statusData, in statusInfos, in context);
			var mainDamage = statusData.MainDamage;
			var totalDamage = mainDamage + statusInfos.StatusController.RemainingTicks;
			var secondDamage = statusData.SecondeDamage;
			var totalSecondeDamage = secondDamage + statusInfos.StatusController.RemainingTicks;
			
			DamageCommand selfDamage = new DamageCommand(totalDamage, statusInfos.targetAddress);
			selfDamage.Run(context.battlePhase);

			if (!statusInfos.targetAddress.TryGetComponentRO(out GridMemberComponent gridMember))
				return;
				
			HexCoordinates center = gridMember.coordinates;
				
			BattleGrid battleGrid = context.battlePhase.BattleGrid;
				
			BattleIgnoreOriginPatternController hexPatternController = new(context.Grid, center);
				
			HexPatternBuilder builder = new HexPatternBuilder(center, hexPatternController) 
				.With(new SpiralPattern(statusData.Range))
				.Without(center);

			foreach (var friend in builder.GetBattleCells(battleGrid))
			{
				DamageCommand friendDamage = new DamageCommand(totalSecondeDamage, friend.EntityAddress);
				friendDamage.Run(context.battlePhase);
			}
		}

		protected override void OnApply(ExplosionStatusData statusStatusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnApply(statusStatusData, in statusInfos, in context);
		}

		protected override void OnRemove(ExplosionStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnRemove(statusData, in statusInfos, in context);
		}
	}
}