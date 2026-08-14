using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.CapacitySystem.Status.Berserk;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct ValkyrieSlash : ICapacity<ValkyrieSlashData>
	{
		private bool asBerserk;
		public void GetHitPattern(ValkyrieSlashData data, ref HexPatternBuilder builder, BattleGrid battleGrid,
			HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			if (!asBerserk)
				builder = builder.With(new SpiralPattern(data.Range)).Without(casterOrigin);
			else
				builder = builder.With(new SpiralPattern(data.BerserkRange)).Without(casterOrigin);
		}

		public void GetTargets(ValkyrieSlashData data, BattleCellAspect battleCell, CapacityTargets output,
			IBattlePlayer castingPlayer)
		{
			foreach (var member in battleCell.GetMembers())
			{
				EntityAddress address = member.EntityAddress;
				if(address.HasComponent<HealthComponent>())
					output.Add(address, CapacityTags.MEMBER);
			}
		}
		private partial void ExecuteSlash(ValkyrieSlashData data, CapacityStepContext ctx)
		{
			asBerserk = false;
			if(ctx.Caster.HasStatus<BerserkStatus>())
				asBerserk = true;
			int ennemyIn = 0;
			foreach (var ctxTarget in ctx.Targets)
			{
				ennemyIn++;
				if (!asBerserk)
				{
					var damage = new DamageCommand(data.Damage, ctxTarget);
					damage.Run(ctx.BattlePhase);

					if (ennemyIn >= data.EnnemyQuantitiesApplyStatus)
					{
						var status = new ApplyStatusCommand(ctx.Caster,data.status);
						status.Run(ctx.BattlePhase);
					}
				}
				else
				{
					var oneDamage = new DamageCommand(data.BerserkRange, ctxTarget);
					oneDamage.Run(ctx.BattlePhase);

					var remove = new RemoveStatusCommand(ctx.Caster, data.status);
					remove.Run(ctx.BattlePhase);
				}
			}
		}
	}
}