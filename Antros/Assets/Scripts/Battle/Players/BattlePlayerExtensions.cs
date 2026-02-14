using ATCG.Battle.Metrics;
using UnityEngine;

namespace ATCG.Battle.Players
{
    public static class BattlePlayerExtensions
    {
        public static string GetPlayerName(this IBattlePlayer player) => player.Profile.Infos.name;
        public static Color GetPlayerColor(this IBattlePlayer player) =>
            GameplayMetrics.Current.GetPlayerColor(player.Profile.ID, player.BattlePhase.PlayerCount);
    }
}