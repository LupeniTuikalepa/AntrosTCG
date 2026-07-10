using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Data.Fire;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct PyroBlessing : ICapacity<PyroBlessingData>
	{
		public HexPatternBuilder GetHitPattern(PyroBlessingData data, BattleGrid battleGrid, HexCoordinates castPoint,
			HexCoordinates casterOrigin)
		{
			BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
			HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
				.With(new PointsPattern(casterOrigin));

			return builder;
		}

		private partial void ExecutePyroBlessing(PyroBlessingData data, CapacityStepContext ctx)
		{
			BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;
			using HexPatternBuilder patternBuilder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);

			foreach (BattleCellAspect cellAspect in patternBuilder.GetBattleCells(battleGrid))
			{
				foreach (var member in cellAspect.GetMembers())
				{
					var statusCommand = new StatusApplyCommand(member.EntityAddress, data.Status);
					statusCommand.Run(ctx.BattlePhase);
				}
			}
		}
	}
}