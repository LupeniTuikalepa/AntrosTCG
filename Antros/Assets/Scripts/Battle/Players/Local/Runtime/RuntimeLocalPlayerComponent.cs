using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Runtime;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime
{
    public abstract class RuntimeLocalPlayerComponent : MonoBehaviour, IRuntimeBattlePlayerComponent<LocalBattlePlayer>
    {
        public LocalBattlePlayer Player => RuntimeLocalPlayer.Player;

        public RuntimeLocalBattlePlayer RuntimeLocalPlayer { get; private set; }
        public BattlePhase BattlePhase => Player.BattlePhase;
        public RuntimeEntityManager RuntimeEntityManager => RuntimeLocalPlayer.RuntimeEntityManager;

        public void Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            Debug.Log($"Connected with player {player.ID}", runtimeBattlePlayer);

            if (runtimeBattlePlayer is RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
            {
                RuntimeLocalPlayer = runtimeLocalBattlePlayer;
                Connect(player);
            }
        }

        public void Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            if (runtimeBattlePlayer is RuntimeLocalBattlePlayer runtimeLocalBattlePlayer &&
                runtimeLocalBattlePlayer == RuntimeLocalPlayer)
            {
                Disconnect(player);
                RuntimeLocalPlayer = null;
            }
        }


        protected abstract void Connect(LocalBattlePlayer player);
        protected abstract void Disconnect(LocalBattlePlayer player);

        public bool IsPlayerTurn() => BattlePhase.CurrentPlayer == Player;

    }
}