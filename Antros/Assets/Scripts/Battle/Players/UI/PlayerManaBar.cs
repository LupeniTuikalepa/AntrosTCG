using ATCG.UI;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ATCG.Battle.Players.Runtime.UI
{
    public class PlayerManaBar : BarUI, IPlayerStatUI
    {
        public void Connect(IBattlePlayer player)
        {
            player.OnPlayerManaChanges += Refresh;
            Refresh(player, player.CurrentHealth, player.CurrentHealth);
        }

        public void Disconnect(IBattlePlayer player)
        {
            player.OnPlayerManaChanges -= Refresh;
        }

        private void Refresh(IBattlePlayer player, int current, int last)
        {
            MaxValue = player.MaxMana;
            CurrentValue = current;
        }
    }
}