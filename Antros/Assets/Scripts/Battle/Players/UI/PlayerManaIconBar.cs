using Helteix.Tools;
using TMPro;
using UnityEngine;

namespace ATCG.Battle.Players.Runtime.UI
{
    public class PlayerManaIconBar : MonoBehaviour, IPlayerStatUI
    {
        [SerializeField]
        private TMP_Text valueText;

        [SerializeField]
        private PlayerManaIcon iconPrefab;
        [SerializeField]
        private Transform container;

        private PlayerManaIcon[] manaIcons;


        public void Connect(IBattlePlayer player)
        {
            container.ClearChildren();

            manaIcons = new PlayerManaIcon[player.MaxMana];
            for (int i = 0; i < player.MaxMana; i++)
                manaIcons[i] = iconPrefab.InstantiatePrefab(container);

            player.OnPlayerManaChanges += Refresh;

            Refresh(player, player.CurrentMana, player.MaxMana);
        }

        public void Disconnect(IBattlePlayer player)
        {
            player.OnPlayerManaChanges -= Refresh;
        }

        private void Refresh(IBattlePlayer player, int current, int last)
        {
            for (int i = 0; i < player.MaxMana; i++)
            {
                var icon = manaIcons[i];
                if(i < current)
                    icon.Activate();
                else
                    icon.Deactivate();
            }

            valueText.text = $"{current}/{player.MaxMana}";
        }
    }
}