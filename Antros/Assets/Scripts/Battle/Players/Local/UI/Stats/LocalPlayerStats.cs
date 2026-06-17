using ATCG.Battle.Players.UI;
using UnityEngine;

namespace ATCG.Battle.Players.Local.UI.Stats
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/LocalPlayerStats")]
    public class LocalPlayerStats : PlayerHUDElement
    {
        [SerializeField]
        private PlayerHealthBar healthBar;

        [SerializeField]
        private ListenerManaIconBar manaBar;

        protected override void OnConnect()
        {
            healthBar.Connect(LocalPlayer);
            manaBar.Connect(LocalPlayer);
        }

        protected override void OnDisconnect()
        {
            healthBar.Disconnect(LocalPlayer);
            manaBar.Disconnect(LocalPlayer);
        }
    }
}