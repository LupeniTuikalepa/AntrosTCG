using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities.Flux
{
	public partial struct Quicksand : ICapacity<QuicksandData>
	{
		public HexPatternBuilder GetHitPattern(QuicksandData data, BattleGrid battleGrid, HexCoordinates castPoint,
			HexCoordinates casterOrigin)
		{
			BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);

			HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
				.With(new SpiralPattern(data.Range)).Without(casterOrigin);
			return builder;
		}

		private partial void ExecuteQuicksand(QuicksandData data, CapacityStepContext ctx)
		{
			BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;
			using HexPatternBuilder patternBuilder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);

			foreach (var cellAspect in patternBuilder.GetBattleCells(battleGrid))
			{
				foreach (var member in cellAspect.GetMembers())
				{
					if (!member.EntityAddress.HasComponent<HealthComponent>())
						continue;
					
					if (!ctx.IsAlly(member.EntityAddress))
					{
						var statusCommand = new StatusApplyCommand(member.EntityAddress, data.Status);
						statusCommand.Run(ctx.BattlePhase);
						
						DamageCommand damageCommand = new DamageCommand(data.Damage, member.EntityAddress);
						damageCommand.Run(ctx.BattlePhase);
					}
				}
			}
		}
	}
}