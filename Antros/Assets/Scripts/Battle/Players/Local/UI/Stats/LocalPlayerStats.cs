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
        private PlayerManaIconBar manaBar;

        protected override void OnConnect()
        {
            healthBar.Connect(Player);
            manaBar.Connect(Player);
        }

        protected override void OnDisconnect()
        {
            healthBar.Disconnect(Player);
            manaBar.Disconnect(Player);
        }
    }
}