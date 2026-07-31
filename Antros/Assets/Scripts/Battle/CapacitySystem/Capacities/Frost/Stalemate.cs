using System;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Data.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct Stalemate : ICapacity<StalemateData>
	{
		public void GetHitPattern(StalemateData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder = builder
				.With(new PointsPattern(castPoint));

		}

		public void GetTargets(StalemateData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
		{
			foreach (var member in battleCell.GetMembers())
			{
				if(member.EntityAddress.HasComponent<HealthComponent>())
					output.Add(member.EntityAddress, CapacityTags.MEMBER);
			}
		}

		private partial void ExecuteStalemate(StalemateData data, CapacityStepContext ctx)
		{
			foreach (var member in ctx.Targets)
			{
				if (member.HasComponent<HealthComponent>())
				{
					var statusFirstCommand = new ApplyStatusCommand(member, data.Status);
					statusFirstCommand.Run(ctx.BattlePhase);

					var statusSecondCommand = new ApplyStatusCommand(member, data.Status);
					statusSecondCommand.Run(ctx.BattlePhase);
				}
			}
		}
	}
}