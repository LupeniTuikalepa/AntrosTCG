using ATCG.Battle.Players.Local.Runtime;
using ATCG.Battle.Players.Runtime;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;

namespace ATCG.Battle.Players.Local.Phases
{
    public abstract class LocalPlayerMonoPhaseListener<T> : MonoPhaseListener<T>, ILocalPlayerPhaseListener<T>, IRuntimeBattlePlayerComponent<LocalBattlePlayer> where T : IPhase, ILocalPlayerPhase
    {
        public LocalBattlePlayer LocalBattlePlayer { get; private set; }
        
        public RuntimeLocalBattlePlayer RuntimeBattlePlayer { get; private set; }

        public virtual void Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            LocalBattlePlayer = player;
            RuntimeBattlePlayer = runtimeBattlePlayer as RuntimeLocalBattlePlayer;
        }

        public virtual void Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            if (LocalBattlePlayer != player) 
                return;
            
            LocalBattlePlayer = null;
            RuntimeBattlePlayer = null;
        }
    }
}