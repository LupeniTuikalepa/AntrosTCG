using ATCG.Metrics;
using UnityEngine;

namespace ATCG.Battle.Players
{
    public static class BattlePlayerExtensions
    {
        public static string GetPlayerName(this IBattlePlayer player)
        {
            return player.Profile.Infos.name;
        }

        public static Color GetPlayerColor(this IBattlePlayer player)
        {
            return GameMetrics.Current.GetPlayerColor(player.Profile.ID, player.BattlePhase.PlayerCount);
        }
    }
}