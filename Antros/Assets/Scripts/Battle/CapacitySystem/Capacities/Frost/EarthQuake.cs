using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
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
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct EarthQuake : ICapacity<EarthQuakeData>
	{
		public void GetTargets(EarthQuakeData data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer)
		{
			output.Add(battleCell.EntityAddress, CapacityTags.CELL);
			foreach (var member in battleCell.GetMembers())
				if(member.EntityAddress.HasComponent<HealthComponent>())
				output.Add(member.EntityAddress, CapacityTags.MEMBER);
		}

		public void GetHitPattern(EarthQuakeData data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder
				.With(new SpiralPattern(data.Range))
				.Without(casterOrigin);
		}

		private partial void ExecuteEarthQuake(EarthQuakeData data, CapacityStepContext ctx)
		{
			foreach (var member in ctx.Targets.WithTags(CapacityTags.MEMBER))
			{
				if (!ctx.IsAlly(member))
				{
					DamageCommand damageCommand = new DamageCommand(data.Damage, member);
					damageCommand.Run(ctx.BattlePhase);
				}
			}

			foreach (var cell in  ctx.Targets.WithTags(CapacityTags.CELL))
			{
				var statusCommand = new StatusApplyCommand(cell, data.Status);
				statusCommand.Run(ctx.BattlePhase);

			}
		}
	}
}