using ATCG.UI;
using UnityEngine;

namespace ATCG.Battle.Players.UI
{
    [AddComponentMenu("ATCG/Gameplay/Player/UI/PlayerManaBar")]
    public class PlayerManaBar : BarUI, IPlayerStatUI
    {
        public void Connect(IBattlePlayer player)
        {
            player.OnPlayerManaChanges += Refresh;
            Refresh(player, player.CurrentMana, player.CurrentMana);
        }

        public void Disconnect(IBattlePlayer player)
        {
            player.OnPlayerManaChanges -= Refresh;
        }

        private void Refresh(IBattlePlayer player, int current, int last)
        {
            MaxValue = player.MaxMana;
            CurrentValue = current;

            Refresh();
        }
    }
}