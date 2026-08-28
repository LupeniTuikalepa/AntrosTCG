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
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct FightMadness : ICapacity<FightMadnessData>
	{
		public void GetHitPattern(FightMadnessData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder = builder
				.With(new PointsPattern(castPoint));

		}

		public void GetTargets(FightMadnessData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
		{
			foreach (var member in battleCell.GetMembers())
			{
				EntityAddress address = member.EntityAddress;
				if(address.HasComponent<HealthComponent>())
					output.Add(address, CapacityTags.MEMBER);
			}
		}

		private partial void ExecuteDeployRage(FightMadnessData data, CapacityStepContext ctx)
		{ 
			var statusCommand = new ApplyStatusCommand(ctx.Caster, data.BerserkData);
			statusCommand.Run(ctx.BattlePhase);
		}


		private partial void ExecutePunch(FightMadnessData data, CapacityStepContext ctx)
		{
			foreach (var member in ctx.Targets.WithTags(CapacityTags.MEMBER))
			{
				var damage = new DamageCommand( data.PunchDamage, member);
				damage.Run(ctx.BattlePhase);
			}
		}
	}
}