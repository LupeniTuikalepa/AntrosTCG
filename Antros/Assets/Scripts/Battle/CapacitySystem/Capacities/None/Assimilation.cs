using ATCG.Battle.CapacitySystem.Capacities.Setup;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.CapacitySystem.Core.Setup.CopyCapa;
using ATCG.Battle.CapacitySystem.Core.Setup.SelectCapacities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.Capacities.None;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Capacities.None
{
	public partial struct Assimilation : ICapacity<AssimilationData>
	{
		public void GetHitPattern(AssimilationData data, ref HexPatternBuilder builder, BattleGrid battleGrid,
			HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			builder = builder
				.With(new PointsPattern(castPoint));
		}

		public void GetTargets(AssimilationData data, BattleCellAspect battleCell, CapacityTargets output,
			IBattlePlayer castingPlayer)
		{
			foreach (var componentRef in battleCell.GetMembers())
			{
				output.Add(componentRef.EntityAddress, CapacityTags.MEMBER);
			}
		}
		
		private partial void ExecuteAssimilation(AssimilationData data, CapacityStepContext ctx)
		{
			if (ctx.capacityPhase.TryGetProperty(CopyCapacitySetup.COPIED_CAPACITY, out CapacityData capacityData))
			{
				if (ctx.Caster.TryGetComponentRO(out CapacityCasterComponent capacityCasterComponent))
				{
					capacityCasterComponent.AddCapacity(capacityData);
				}
			}
		}
	}
}