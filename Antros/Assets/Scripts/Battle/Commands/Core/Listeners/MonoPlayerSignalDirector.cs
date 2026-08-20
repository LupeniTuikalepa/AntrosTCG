using System;
using ATCG.Battle.Commands.Players;
using ATCG.Battle.Players;

namespace ATCG.Battle.Commands.Listeners
{
    public abstract class MonoPlayerSignalDirector : 
        MonoBaseSignalDirector<PlayerCommandSignal>,
        IPlayerSignalDirector
    {
        public IBattlePlayer BattlePlayer { get; private set; }

        private void Start()
        {
            BattlePlayer = RuntimeEntity.RuntimeBattlePlayer.BattlePlayer;
        }
    }
}