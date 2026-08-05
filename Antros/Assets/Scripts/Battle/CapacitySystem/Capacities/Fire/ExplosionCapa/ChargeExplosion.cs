using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
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
	public partial struct ChargeExplosion : ICapacity<ChargeExplosionData>
	{
		public void GetHitPattern(ChargeExplosionData data, ref HexPatternBuilder builder, BattleGrid battleGrid,
			HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder = builder
				.With(new PointsPattern(castPoint));
		}

		public void GetTargets(ChargeExplosionData data, BattleCellAspect battleCell, CapacityTargets output,
			IBattlePlayer castingPlayer)
		{
			foreach (var member in battleCell.GetMembers())
			{
				EntityAddress address = member.EntityAddress;
				if(address.IsAlly(castingPlayer) && address.HasComponent<HealthComponent>())
					output.Add(address, CapacityTags.MEMBER);
			}
		}
		private partial void ExecuteCharging(ChargeExplosionData data, CapacityStepContext ctx)
		{
			var applyExplo = new ApplyStatusCommand(ctx.Caster, data.Status);
			applyExplo.Run(ctx.BattlePhase);
		}
	}
}