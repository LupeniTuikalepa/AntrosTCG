using ATCG.Battle.Entities;

namespace ATCG.Battle.Players.Local.Phases
{
	public class HoverEntityPhase : LocalPlayerPhaseCompletionSource<EntityAddress>
	{
		public EntityAddress HoveredAddress { get; private set; }
		
		public HoverEntityPhase(LocalBattlePlayer localBattlePlayers, EntityAddress hoveredAddress) : base(localBattlePlayers)
		{
			HoveredAddress = hoveredAddress;
		}
	}
}
