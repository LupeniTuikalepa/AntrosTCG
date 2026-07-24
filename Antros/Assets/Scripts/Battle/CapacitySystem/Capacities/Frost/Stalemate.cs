using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Data.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct Stalemate : ICapacity<StalemateData>
	{
		public HexPatternBuilder GetHitPattern(StalemateData data, BattleGrid battleGrid, HexCoordinates castPoint,
			HexCoordinates casterOrigin)
		{
			BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
			HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
				.With(new PointsPattern(castPoint));

			return builder;
		}

		private partial void ExecuteStalemate(StalemateData data, CapacityStepContext ctx)
		{
			if (ctx.BattleGrid.TryGetBattleCell(ctx.CastPoint, out var cell))
			{
				foreach (var member in cell.GetMembers())
				{
					if (member.EntityAddress.HasComponent<HealthComponent>())
					{
						var statusFirstCommand = new StatusApplyCommand(member.EntityAddress, data.Status);
						statusFirstCommand.Run(ctx.BattlePhase);
						
						var statusSecondCommand = new StatusApplyCommand(member.EntityAddress, data.Status);
						statusSecondCommand.Run(ctx.BattlePhase);
					}
				}
			}
		}
	}
}