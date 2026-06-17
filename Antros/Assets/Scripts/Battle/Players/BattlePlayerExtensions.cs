using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Metrics;
using UnityEngine;

namespace ATCG.Battle.Players
{
    public static class BattlePlayerExtensions
    {
        public static BattleID GetBattleID(this IBattlePlayer player)
            => player.Profile.ID;

        public static string GetPlayerName(this IBattlePlayer player)
            => player.Profile.Infos.name;

        public static int GetPlayerNumber(this IBattlePlayer player) =>
            player.BattlePhase.GetPlayerNumber(player);

        public static Color GetPlayerColor(this IBattlePlayer player)
        {
            int number = player.BattlePhase.GetPlayerNumber(player);
            return GameMetrics.Current.GetPlayerColor(number, player.BattlePhase.PlayerCount);
        }
    }
}