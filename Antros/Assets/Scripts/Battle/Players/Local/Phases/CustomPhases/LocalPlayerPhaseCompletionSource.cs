using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.Phases
{
    public abstract class LocalPlayerPhaseCompletionSource<T> : PhaseCompletionSource<T>, ILocalPlayerPhase
    {
        public LocalBattlePlayer LocalBattlePlayer { get; }

        public LocalPlayerPhaseCompletionSource(LocalBattlePlayer localBattlePlayer)
        {
            LocalBattlePlayer = localBattlePlayer;
        }
    }
}