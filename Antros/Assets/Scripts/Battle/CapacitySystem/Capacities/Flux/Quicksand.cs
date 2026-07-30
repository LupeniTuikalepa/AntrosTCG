using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities.Flux
{
	public partial struct Quicksand : ICapacity<QuicksandData>
	{
		[CapacityTargetTag]
		public const string APPLY_STATUS = nameof(APPLY_STATUS);
		[CapacityTargetTag]
		public const string DAMAGE = nameof(DAMAGE);

		public void GetHitPattern(QuicksandData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
		{

			builder = builder
				.With(new SpiralPattern(data.Range)).Without(casterOrigin);
		}

		public void GetTargets(QuicksandData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
		{
			foreach (var member in battleCell.GetMembers())
			{
				if(member.EntityAddress.IsAlly(castingPlayer))
					continue;

				if(member.EntityAddress.HasComponent<StatusReceiver>())
					output.Add(member.EntityAddress, APPLY_STATUS);

				if(member.EntityAddress.HasComponent<HealthComponent>())
					output.Add(member.EntityAddress, DAMAGE);
			}
		}

		private partial void ExecuteQuicksand(QuicksandData data, CapacityStepContext ctx)
		{
			foreach (var target in ctx.Targets.WithTags(APPLY_STATUS))
			{
				StatusApplyCommand statusCommand = new StatusApplyCommand(target, data.Status);
				statusCommand.Run(ctx.BattlePhase);
			}

			foreach (var target in ctx.Targets.WithTags(DAMAGE))
			{
				DamageCommand damageCommand = new DamageCommand(data.Damage, target);
				damageCommand.Run(ctx.BattlePhase);
			}
		}
	}
}