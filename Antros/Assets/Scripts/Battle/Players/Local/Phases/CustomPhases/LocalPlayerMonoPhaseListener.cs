using ATCG.Battle.Players.Runtime;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;

namespace ATCG.Battle.Players.Local.Phases
{
    public abstract class LocalPlayerMonoPhaseListener<T> : MonoPhaseListener<T>, ILocalPlayerPhaseListener<T>, IRuntimeBattlePlayerComponent<LocalBattlePlayer> where T : IPhase, ILocalPlayerPhase
    {
        public LocalBattlePlayer LocalBattlePlayer { get; protected set; }

        public virtual void Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            LocalBattlePlayer = player;
        }

        public virtual void Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            if (LocalBattlePlayer == player)
                LocalBattlePlayer = null;
        }
    }
}