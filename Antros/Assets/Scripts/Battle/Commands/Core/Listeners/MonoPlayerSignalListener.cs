using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Listeners
{
    public abstract class MonoPlayerSignalListener : 
        MonoBaseSignalListener<PlayerCommandSignal>,
        IPlayerSignalListener
    {
        public IBattlePlayer BattlePlayer { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            BattlePlayer = RuntimeEntity.RuntimeBattlePlayer.BattlePlayer;
        }
    }
}