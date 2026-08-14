using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local;
using ATCG.HexGrids;

namespace ATCG.Capacities.Setup
{
	public struct CapacitySetupContext
	{
		public BattlePhase battlePhase;
		public CapacityTargets targets;
		public LocalBattlePlayer player;
		public CapacityData data;
		public EntityAddress caster;
		public HexCoordinates castPoints;
		public CastCapacityPhase castCapacityPhase;
	}
}