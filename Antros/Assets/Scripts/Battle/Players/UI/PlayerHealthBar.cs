using ATCG.UI;
using UnityEngine;

namespace ATCG.Battle.Players.UI
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/PlayerHealthBar")]
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
            Refresh();
        }
    }
}