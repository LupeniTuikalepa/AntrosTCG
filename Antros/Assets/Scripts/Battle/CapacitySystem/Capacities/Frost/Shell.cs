using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
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
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct Shell : ICapacity<ShellData>
	{
		[CapacityTargetTag]
		public const string StatusTarget = nameof(StatusTarget);

		public void GetHitPattern(ShellData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder
				.With(new SpiralPattern(data.Range));
		}

		public void GetTargets(ShellData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
		{
			foreach (var member in battleCell.GetMembers())
			{
				if (member.EntityAddress.HasComponent<DefenseComponent>())
					output.Add(member.EntityAddress, CapacityTags.MEMBER);
			}
		}


		private partial void ExecuteShell(ShellData data, CapacityStepContext ctx)
		{
			foreach (var member in ctx.Targets)
			{
				if (ctx.IsAlly(member))
				{
					var statusCommand = new ApplyStatusCommand(member, data.Status);
					statusCommand.Run(ctx.BattlePhase);
					Debug.Log(member.entity);
				}
			}
		}
	}
}