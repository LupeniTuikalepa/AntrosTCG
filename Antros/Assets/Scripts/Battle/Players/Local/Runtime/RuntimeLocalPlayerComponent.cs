using ATCG.Battle.Players.Runtime;
using UnityEngine;

namespace ATCG.Battle.Players.Local
{
    public abstract class RuntimeLocalPlayerComponent : MonoBehaviour, IRuntimeBattlePlayerComponent<LocalBattlePlayer>
    {
        public LocalBattlePlayer Player => RuntimeLocalPlayer.Player;

        public RuntimeLocalBattlePlayer RuntimeLocalPlayer { get; private set; }

        protected virtual void Awake()
        {
            RuntimeLocalPlayer = GetComponentInParent<RuntimeLocalBattlePlayer>();
        }


        protected abstract void Connect(LocalBattlePlayer player);
        protected abstract void Disconnect(LocalBattlePlayer player);

        public void Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            if (runtimeBattlePlayer is RuntimeLocalBattlePlayer runtimeLocalBattlePlayer)
            {
                RuntimeLocalPlayer = runtimeLocalBattlePlayer;
                Connect(player);
            }
        }

        public void Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer battlePlayer)
        {
            if (runtimeBattlePlayer is RuntimeLocalBattlePlayer runtimeLocalBattlePlayer && runtimeLocalBattlePlayer == RuntimeLocalPlayer)
            {
                Disconnect(battlePlayer);
                RuntimeLocalPlayer = null;
            }
        }
    }
}