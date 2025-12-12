using ATCG.UI;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Players.Runtime.UI
{
    public class PlayerHealthBar : BarUI, IPlayerStatUI
    {
        public void Connect(IBattlePlayer player)
        {
            player.OnPlayerHealthChanges += Refresh;
            Refresh(player, player.CurrentHealth, player.CurrentHealth);
        }

        public void Disconnect(IBattlePlayer player)
        {
            player.OnPlayerHealthChanges -= Refresh;
        }

        private void Refresh(IBattlePlayer player, int current, int last)
        {
            MaxValue = player.MaxHealth;
            CurrentValue = current;
        }
    }
}