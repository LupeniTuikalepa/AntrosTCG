using System;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Listeners
{
    public abstract class MonoPlayerSignalListener : 
        MonoBaseSignalListener<PlayerCommandSignal>,
        IPlayerSignalListener
    {
        public IBattlePlayer BattlePlayer { get; private set; }

        private void Start()
        {
            BattlePlayer = RuntimeEntity.RuntimeBattlePlayer.BattlePlayer;
        }
    }
}