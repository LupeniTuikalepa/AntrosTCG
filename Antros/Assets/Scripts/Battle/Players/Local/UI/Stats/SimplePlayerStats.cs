using TMPro;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime.UI
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/SimplePlayerStats")]
    public class SimplePlayerStats : MonoBehaviour
    {
        [SerializeField]
        private PlayerHealthBar healthBar;
        [SerializeField]
        private PlayerManaBar manaBar;
        [SerializeField]
        private TMP_Text playerName;

        public virtual void Connect(IBattlePlayer player)
        {
            playerName.text = player.GetPlayerName();
            playerName.color = player.GetPlayerColor();

            healthBar.Connect(player);
            manaBar.Connect(player);
        }

        public virtual void Disconnect(IBattlePlayer player)
        {
            playerName.text = string.Empty;
            healthBar.Disconnect(player);
            manaBar.Disconnect(player);
        }
    }
}