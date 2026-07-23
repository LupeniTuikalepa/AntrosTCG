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
	public partial class ExplosionStatus : Status<ExplosionData,StatusDurationController>
	{
		protected override StatusDurationController CreateStatusController(ExplosionData data, in StatusContext context)
		{
			return new StatusDurationController(data.Duration);
		}

		protected override void OnStack(ExplosionData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnStack(data, in statusInfos, in context);
			ref StatusDurationController controller = ref statusInfos.statusControllerRef.GetValue();
			if (controller.RemainingTicks < data.Duration)
			{
				controller.SetTicks(data.Duration);
			}
		}

		protected override void OnTick(ExplosionData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnTick(data, in statusInfos, in context);
			DamageCommand selfDamage = new DamageCommand(data.MainDamage, statusInfos.targetAddress);
			selfDamage.Run(context.battlePhase);

			if (!statusInfos.targetAddress.TryGetComponentRO(out GridMemberComponent gridMember))
				return;
				
			HexCoordinates center = gridMember.coordinates;
				
			BattleGrid battleGrid = context.battlePhase.BattleGrid;
				
			BattleIgnoreOriginPatternController hexPatternController = new(context.Grid, center);
				
			HexPatternBuilder builder = new HexPatternBuilder(center, hexPatternController) 
				.With(new SpiralPattern(data.Range))
				.Without(center);

			foreach (var friend in builder.GetBattleCells(battleGrid))
			{
				DamageCommand friendDamage = new DamageCommand(data.SecondeDamage, friend.EntityAddress);
				friendDamage.Run(context.battlePhase);
			}
		}

		protected override void OnApply(ExplosionData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnApply(data, in statusInfos, in context);
		}

		protected override void OnRemove(ExplosionData data, in EntityStatusInfos statusInfos, in StatusContext context)
		{
			base.OnRemove(data, in statusInfos, in context);
		}
	}
}