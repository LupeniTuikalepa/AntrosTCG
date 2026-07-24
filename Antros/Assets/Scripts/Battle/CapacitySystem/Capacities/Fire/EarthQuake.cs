using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Commands;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Controllers;
using ATCG.Capacities.Data.Fire;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Capacities
{
	public partial struct EarthQuake : ICapacity<EarthQuakeData>
	{
		public HexPatternBuilder GetHitPattern(EarthQuakeData data, BattleGrid battleGrid, HexCoordinates castPoint,
			HexCoordinates casterOrigin)
		{
			BattleIgnoreOriginPatternController hexPatternController = new(battleGrid, castPoint);

			HexPatternBuilder builder = new HexPatternBuilder(castPoint, hexPatternController)
				.With(new SpiralPattern(data.Range))
				.Without(casterOrigin);
			
			return builder;
		}

		private partial void ExecuteEarthQuake(EarthQuakeData data, CapacityStepContext ctx)
		{
			BattleGrid battleGrid = ctx.BattlePhase.BattleGrid;
			using HexPatternBuilder patternBuilder = GetHitPattern(data, battleGrid, ctx.CastPoint, ctx.capacityPhase.CasterOrigin);

			foreach (var cellAspect in patternBuilder.GetBattleCells(battleGrid))
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
				}
			}
		}
	}
}