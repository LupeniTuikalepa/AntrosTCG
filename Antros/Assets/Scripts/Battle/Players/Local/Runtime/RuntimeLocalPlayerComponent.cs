using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Runtime;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime
{
    public abstract class RuntimeLocalPlayerComponent : MonoBehaviour, IRuntimeBattlePlayerComponent<LocalBattlePlayer>
    {
        public bool IsConnected => RuntimeLocalBattlePlayer != null;

        protected LocalBattlePlayer Player => RuntimeLocalBattlePlayer.BattlePlayer;
        protected RuntimeLocalBattlePlayer RuntimeLocalBattlePlayer { get; private set; }


        protected BattlePhase BattlePhase => Player.BattlePhase;
        protected RuntimeEntityManager RuntimeEntityManager => RuntimeLocalBattlePlayer.RuntimeEntityManager;

        public void Connect(IRuntimeBattlePlayer<LocalBattlePlayer> runtimeBattlePlayer)
        {
            if (runtimeBattlePlayer is RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
            {
                RuntimeLocalBattlePlayer = runtimeLocalBattlePlayer;
                Connect(runtimeLocalBattlePlayer);
            }
        }

        public void Disconnect(IRuntimeBattlePlayer<LocalBattlePlayer> runtimeBattlePlayer)
        {
            if (runtimeBattlePlayer is RuntimeLocalBattlePlayer runtimeLocalBattlePlayer &&
                runtimeLocalBattlePlayer == RuntimeLocalBattlePlayer)
            {
                Disconnect(runtimeLocalBattlePlayer);
                RuntimeLocalBattlePlayer = null;
            }
        }


        protected abstract void Connect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer);
        protected abstract void Disconnect(RuntimeLocalBattlePlayer runtimeLocalBattlePlayer);

        public bool IsPlayerTurn() => BattlePhase.CurrentPlayer == Player;

    }
}