using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Data.Frost;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct Quickness: ICapacity<QuicknessData>
	{
		public void GetHitPattern(QuicknessData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint,
			HexCoordinates casterOrigin)
		{
			builder = builder
				.With(new PointsPattern(castPoint));
		}

		public void GetTargets(QuicknessData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
		{
			foreach (var member in battleCell.GetMembers())
			{
				if(member.EntityAddress.HasComponent<HealthComponent>())
					output.Add(member.EntityAddress, CapacityTags.MEMBER);
			}
		}

		private partial void ExecuteQuickness(QuicknessData data, CapacityStepContext ctx)
		{
			foreach (var member in ctx.Targets)
			{
				if (member.HasComponent<HealthComponent>())
				{
					var statusFirstCommand = new StatusApplyCommand(member, data.Status);
					statusFirstCommand.Run(ctx.BattlePhase);
				}

			}
		}
	}
}