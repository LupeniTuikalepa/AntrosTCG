using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.CapacitySystem.Status.Frost.EarthQuake;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Data.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct EarthFracture :ICapacity<EarthFractureData> 
	{
		public HexPatternBuilder GetHitPattern(EarthFractureData data, BattleGrid battleGrid, HexCoordinates castPoint,
			HexCoordinates casterOrigin)
		{
			BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
			HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
				.With(new PointsPattern(castPoint));

			return builder;
		}

		private partial void ExecuteEarthFracture(EarthFractureData data, CapacityStepContext ctx)
		{
			BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;
			using HexPatternBuilder patternBuilder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);
			
			foreach (var cellAspect in patternBuilder.GetBattleCells(battleGrid))
			{
				if (!cellAspect.EntityAddress.HasStatus<EarthQuakeStatus>())
				{
					var statusCommand = new StatusApplyCommand(cellAspect.EntityAddress, data.Status);
					statusCommand.Run(ctx.BattlePhase);
				}
				else
				{
					HexCoordinates cellCenter = cellAspect.Coordinate;
					BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, cellCenter);
					HexPatternBuilder pattern = new HexPatternBuilder(cellCenter,hexPatternController).With(new SpiralPattern(1))
						.Without(cellCenter);

					foreach (var friend in pattern.GetBattleCells(battleGrid))
					{
						var statusCommand = new StatusApplyCommand(friend.EntityAddress, data.Status);
						statusCommand.Run(ctx.BattlePhase);
					}
				}
			}
		}
	}
}