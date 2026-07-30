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
			ref StatusDurationController controller = ref statusInfos.statusControllerRef.GetValue();
			if (controller.RemainingTicks < statusData.Duration)
			{
				controller.SetTicks(statusData.Duration);
			}
		}

		protected override void OnTick(ExplosionStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnTick(statusData, in statusInfos, in context);
			DamageCommand selfDamage = new DamageCommand(statusData.MainDamage, statusInfos.targetAddress);
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
				DamageCommand friendDamage = new DamageCommand(statusData.SecondeDamage, friend.EntityAddress);
				friendDamage.Run(context.battlePhase);
			}
		}

		protected override void OnApply(ExplosionStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnApply(statusData, in statusInfos, in context);
		}

		protected override void OnRemove(ExplosionStatusData statusData, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnRemove(statusData, in statusInfos, in context);
		}
	}
}