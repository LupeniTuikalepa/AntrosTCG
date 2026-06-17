using ATCG.Battle.Players.Local.Runtime;
using UnityEngine;

namespace ATCG.Battle.Players.Local.UI
{
    public abstract class PlayerHUDElement : MonoBehaviour
    {
        public PlayerHUD HUD { get; private set; }
        public RuntimeLocalBattlePlayer RuntimePlayer { get; private set; }

        public LocalBattlePlayer LocalPlayer => RuntimePlayer.BattlePlayer;

        public void Initialize(PlayerHUD hud)
        {
            HUD = hud;
        }

        public void Connect(RuntimeLocalBattlePlayer player)
        {
            if (RuntimePlayer != null)
                Disconnect();

            RuntimePlayer = player;
            OnConnect();
        }

        public void Disconnect()
        {
            if (RuntimePlayer == null)
                return;

            OnDisconnect();
            RuntimePlayer = null;
        }

        protected abstract void OnConnect();
        protected abstract void OnDisconnect();
    }
}