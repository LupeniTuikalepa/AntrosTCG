using ATCG.Battle.Turns;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players
{
    public abstract class PlayerTurnPhase : PhaseCompletionSource<BattleTurn>
    {
        public readonly IBattlePlayer player;
        public readonly int turnNumber;

        protected PlayerTurnPhase(int turnNumber, IBattlePlayer player)
        {
            this.turnNumber = turnNumber;
            this.player = player;
        }
    }
}