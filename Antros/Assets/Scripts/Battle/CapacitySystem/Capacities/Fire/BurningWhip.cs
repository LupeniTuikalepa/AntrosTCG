using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
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
		public HexPatternBuilder GetHitPattern(BurningWhipData data, BattleGrid battleGrid, HexCoordinates castPoint,
			HexCoordinates casterOrigin)
		{
			BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);
			HexPatternBuilder builder = new HexPatternBuilder(casterOrigin, hexPatternController)
				.With(new TridentPattern(castPoint, data.TridentPatternData), casterOrigin)
				.Without(casterOrigin);

			return builder;
		}

		private partial void ExecuteBurningWhip(BurningWhipData data, CapacityStepContext ctx)
		{
			BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;
			using HexPatternBuilder patternBuilder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);

			foreach (BattleCellAspect cellAspect in patternBuilder.GetBattleCells(battleGrid))
			{
				if (data.Status.TryGet(out IStatusContainer groundStatusContainer))
				{
					var statusCommand = new StatusApplyCommand(cellAspect.EntityAddress, data.Status);
					statusCommand.Run(ctx.BattlePhase);
				}

				foreach (var member in cellAspect.GetMembers())
				{
					if (!member.EntityAddress.HasComponent<HealthComponent>())
						continue;

					if (!ctx.IsAlly(member.EntityAddress))
					{
						DamageCommand damageCommand = new DamageCommand(data.Damage, member.EntityAddress);
						damageCommand.Run(ctx.BattlePhase);
					}
					if (data.Status.TryGet(out IStatusContainer unitStatusContainer))
					{
						var statusCommand = new StatusApplyCommand(member.EntityAddress, data.Status);
						statusCommand.Run(ctx.BattlePhase);
					}
				}
			}
		}
	}
}