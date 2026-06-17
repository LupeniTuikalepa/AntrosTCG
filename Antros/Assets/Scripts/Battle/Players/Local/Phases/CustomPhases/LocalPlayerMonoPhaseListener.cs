using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;

namespace ATCG.Battle.Players.Local.Phases
{
    public abstract class LocalPlayerMonoPhaseListener<T> : MonoPhaseListener<T>, ILocalPlayerPhaseListener<T>, IRuntimeBattlePlayerComponent<LocalBattlePlayer> where T : IPhase, ILocalPlayerPhase
    {
        public RuntimeLocalBattlePlayer RuntimeBattlePlayer { get; private set; }
        public LocalBattlePlayer LocalBattlePlayer => RuntimeBattlePlayer.BattlePlayer;

        public virtual void Connect(IRuntimeBattlePlayer<LocalBattlePlayer> runtimeBattlePlayer)
        {
            RuntimeBattlePlayer = runtimeBattlePlayer as RuntimeLocalBattlePlayer;
        }

        public virtual void Disconnect(IRuntimeBattlePlayer<LocalBattlePlayer> runtimeBattlePlayer)
        {
            if (LocalBattlePlayer != runtimeBattlePlayer.BattlePlayer)
                return;

            RuntimeBattlePlayer = null;
        }
    }
}