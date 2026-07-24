using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Data.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct Shell : ICapacity<ShellData>
	{
		public HexPatternBuilder GetHitPattern(ShellData data, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);

			HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
				.With(new SpiralPattern(data.Range));
			return builder;
		}

		private partial void ExecuteShell(ShellData data, CapacityStepContext ctx)
		{
			BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;
			using HexPatternBuilder patternBuilder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);

			foreach (var cellAspect in patternBuilder.GetBattleCells(battleGrid))
			{
				foreach (var member in cellAspect.GetMembers())
				{
					if (!member.EntityAddress.HasComponent<HealthComponent>())
						continue;
					
					if (ctx.IsAlly(member.EntityAddress))
					{
						var statusCommand = new StatusApplyCommand(member.EntityAddress, data.Status);
						statusCommand.Run(ctx.BattlePhase);
					}
				}
			}
		}
	}
}