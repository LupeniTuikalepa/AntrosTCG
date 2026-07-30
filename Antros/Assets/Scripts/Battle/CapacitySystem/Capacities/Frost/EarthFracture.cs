using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.CapacitySystem.Status.Frost.EarthQuake;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct EarthFracture :ICapacity<EarthFractureData>
	{
		[CapacityTargetTag]
		public const string APPLY_STATUS = nameof(APPLY_STATUS);

		public void GetHitPattern(EarthFractureData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder.With(new PointsPattern(castPoint));
			if (battleGrid.TryGetBattleCell(castPoint, out var cell))
			{
				if (cell.EntityAddress.HasStatus<EarthQuakeStatus>())
					builder.With(new SpiralPattern(data.EarthQuakePropagationRange));
			}
		}

		public void GetTargets(EarthFractureData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
		{
			output.Add(battleCell.EntityAddress, APPLY_STATUS);
		}

		private partial void ExecuteEarthFracture(EarthFractureData data, CapacityStepContext ctx)
		{
			foreach (EntityAddress target in ctx.Targets.WithTags(APPLY_STATUS))
			{
				var statusCommand = new StatusApplyCommand(target, data.Status);
				statusCommand.Run(ctx.BattlePhase);
			}
		}
	}
}