using UnityEngine;

namespace ATCG.Battle.Players.Local.UI
{
    public abstract class PlayerHUDElement : MonoBehaviour
    {
        public PlayerHUD HUD { get; private set; }
        public LocalBattlePlayer Player { get; private set; }

        public void Initialize(PlayerHUD hud)
        {
            HUD = hud;
        }

        public void Connect(LocalBattlePlayer player)
        {
            if (Player != null)
                Disconnect();

            Player = player;
            OnConnect();
        }

        public void Disconnect()
        {
            if (Player == null)
                return;

            OnDisconnect();
            Player = null;
        }

        protected abstract void OnConnect();
        protected abstract void OnDisconnect();
    }
}