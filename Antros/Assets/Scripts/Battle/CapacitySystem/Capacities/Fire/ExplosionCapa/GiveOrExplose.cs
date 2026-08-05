using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Data.Fire.ExplosionData;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities.ExplosionCapa
{
	public partial struct GiveOrExplose : ICapacity<GiveOrExploseData>
	{
		public void GetHitPattern(GiveOrExploseData data, ref HexPatternBuilder builder, BattleGrid battleGrid,
			HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder = builder
				.With(new PointsPattern(castPoint));
		}

		public void GetTargets(GiveOrExploseData data, BattleCellAspect battleCell, CapacityTargets output,
			IBattlePlayer castingPlayer)
		{
			foreach (var member in battleCell.GetMembers())
			{
				EntityAddress address = member.EntityAddress;
				if(address.HasComponent<HealthComponent>())
					output.Add(address, CapacityTags.MEMBER);
			}
		}

		private partial void ExecuteHit(GiveOrExploseData data, CapacityStepContext ctx)
		{
			foreach (var member in ctx.Targets.WithTags(CapacityTags.MEMBER))
			{
				var damage = new DamageCommand( data.PunchDamage, member);
				damage.Run(ctx.BattlePhase);
			}
		}

		private partial void ExecuteGiveOrExplose(GiveOrExploseData data, CapacityStepContext ctx)
		{
			var statusCommand = new ApplyStatusCommand(ctx.Caster, data.Status);
			statusCommand.Run(ctx.BattlePhase);
		}
	}
}