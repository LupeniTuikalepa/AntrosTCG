using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities.None;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Capacities.None
{
	public partial struct Assimilation : ICapacity<AssimilationData>
	{
		public void GetHitPattern(AssimilationData data, ref HexPatternBuilder builder, BattleGrid battleGrid,
			HexCoordinates castPoint, HexCoordinates casterOrigin)
		{
			
		}

		public void GetTargets(AssimilationData data, BattleCellAspect battleCell, CapacityTargets output,
			IBattlePlayer castingPlayer)
		{
			
		}

		private partial void ExecuteAssimilation(AssimilationData data, CapacityStepContext ctx)
		{
		}
	}
}