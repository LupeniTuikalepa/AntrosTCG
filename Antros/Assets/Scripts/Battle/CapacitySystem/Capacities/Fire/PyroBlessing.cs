using System;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.Data.Fire;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct PyroBlessing : ICapacity<PyroBlessingData>
	{
		public void GetHitPattern(PyroBlessingData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder = builder
				.With(new PointsPattern(castPoint));

		}

		public void GetTargets(PyroBlessingData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
		{
			foreach (var member in battleCell.GetMembers())
			{
				if(member.EntityAddress.IsAlly(castingPlayer) && member.EntityAddress.HasComponent<StatusReceiver>())
					output.Add(member.EntityAddress, CapacityTags.MEMBER);
			}
		}

		private partial void ExecutePyroBlessing(PyroBlessingData data, CapacityStepContext ctx)
		{
			foreach (var target in ctx.Targets.WithTags(CapacityTags.MEMBER))
			{
				var statusCommand = new ApplyStatusCommand(target, data.Status);
				statusCommand.Run(ctx.BattlePhase);
			}
		}
	}
}