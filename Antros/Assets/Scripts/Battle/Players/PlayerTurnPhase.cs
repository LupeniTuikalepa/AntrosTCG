using ATCG.Battle.Actions;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players
{
    public abstract class PlayerTurnPhase : PhaseCompletionSource<BattleTurn>
    {
        public readonly int turnNumber;
        public readonly IBattlePlayer player;

        protected PlayerTurnPhase(int turnNumber, IBattlePlayer player)
        {
            this.turnNumber = turnNumber;
            this.player = player;
        }
    }
}