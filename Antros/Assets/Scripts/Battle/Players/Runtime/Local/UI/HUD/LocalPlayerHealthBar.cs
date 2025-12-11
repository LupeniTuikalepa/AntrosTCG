using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Players.Runtime.Local.UI.HUD
{
    public class LocalPlayerHealthBar : MonoBehaviour, IRuntimeLocalHUDElement
    {
        public RuntimeLocalHUD HUD { get; set; }

        [SerializeField]
        private TMP_Text valueText;
        [SerializeField]
        private Image fill;

        [SerializeField]
        private float fillDuration = .2f;

        public void Connect(RuntimeLocalBattlePlayer runtimePlayer, LocalBattlePlayer player)
        {
            player.OnPlayerHealthChanges += Refresh;

            Refresh(player, player.CurrentHealth, player.CurrentHealth);
        }

        public void Disconnect(RuntimeLocalBattlePlayer runtimePlayer, LocalBattlePlayer player)
        {
            player.OnPlayerHealthChanges -= Refresh;
        }
        private void Refresh(LocalBattlePlayer player, int current, int last)
        {
            float target = current / (float)player.MaxHealth;

            Tween.StopAll(fill);
            Tween.UIFillAmount(fill, target, fillDuration, Ease.OutCubic);
            valueText.text = $"{current}/{player.MaxHealth}";
        }
    }
}