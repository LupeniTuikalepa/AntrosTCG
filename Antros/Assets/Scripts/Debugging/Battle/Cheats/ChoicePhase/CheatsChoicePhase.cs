using System.Collections.Generic;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;

namespace ATCG.Debugging.Debugging.Battle.ChoicePhase
{
	public class CheatsChoicePhase : LocalPlayerPhaseCompletionSource<string>
	{
		public readonly List<string> choices;

		public CheatsChoicePhase(LocalBattlePlayer localBattlePlayer, List<string> choices) : base(localBattlePlayer)
		{
			this.choices = choices;
		}
	}
}