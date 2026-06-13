using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.Phases
{
    public abstract class LocalPlayerPhase : Phase, ILocalPlayerPhase
    {
        public LocalBattlePlayer LocalBattlePlayer { get; }

        public LocalPlayerPhase(LocalBattlePlayer localBattlePlayer)
        {
            LocalBattlePlayer = localBattlePlayer;
        }
    }
}