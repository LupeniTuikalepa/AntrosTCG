using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Attributs;
using ATCG.Capacities.Data.Fire;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct BurningWhip : ICapacity<BurningWhipData>
	{
		[CapacityTargetTag]
		public const string DAMAGE = nameof(DAMAGE);
		[CapacityTargetTag]
		public const string APPLY_BURNING_STATUS = nameof(DAMAGE);
		[CapacityTargetTag]
		public const string APPLY_INCANDESCENCE_STATUS = nameof(DAMAGE);

		public void GetHitPattern(BurningWhipData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder = builder
				.With(new TridentPattern(castPoint, data.TridentPatternData), casterOrigin)
				.Without(casterOrigin);
		}

		public void GetTargets(BurningWhipData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
		{
			output.Add(battleCell.EntityAddress, APPLY_INCANDESCENCE_STATUS);
			foreach (var member in battleCell.GetMembers())
			{
				if (!member.EntityAddress.IsAlly(castingPlayer))
				{
					if (member.EntityAddress.HasComponent<HealthComponent>())
						output.Add(member.EntityAddress, DAMAGE);

					output.Add(member.EntityAddress, APPLY_INCANDESCENCE_STATUS);
				}
			}
		}

		private partial void ExecuteBurningWhip(BurningWhipData data, CapacityStepContext ctx)
		{
			foreach (var target in ctx.Targets.WithTags(APPLY_BURNING_STATUS))
			{
				var statusCommand = new ApplyStatusCommand(target, data.BurningStatus);
				statusCommand.Run(ctx.BattlePhase);
			}
			foreach (var target in ctx.Targets.WithTags(APPLY_INCANDESCENCE_STATUS))
			{
				var statusCommand = new ApplyStatusCommand(target, data.IncandescenceStatus);
				statusCommand.Run(ctx.BattlePhase);
			}
			foreach (var target in ctx.Targets.WithTags(DAMAGE))
			{
				DamageCommand damageCommand = new DamageCommand(data.Damage, target);
				damageCommand.Run(ctx.BattlePhase);
			}
		}
	}
}