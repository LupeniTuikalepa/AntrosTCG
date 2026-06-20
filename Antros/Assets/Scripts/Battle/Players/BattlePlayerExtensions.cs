using System.Collections.Generic;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns;
using ATCG.HexGrids.Patterns.Building;
using ATCG.HexGrids.Utility;
using ATCG.Metrics;
using UnityEngine;

namespace ATCG.Battle.Players
{
    public static class BattlePlayerExtensions
    {
        private static readonly HexCoordinates[] Corners = new HexCoordinates[HexOperations.DirectionsCount];

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

        public static void GetStartingLine<T>(this IBattlePlayer battlePlayer, HexPatternBuilder<T> builder)
            where T : IHexPatternController
        {
            HexCoordinates center = new HexCoordinates(0, 0);
            for (int i = 0; i < HexOperations.DirectionsCount; i++)
            {
                HexCoordinates corner = HexOperations.GetNeighbor(center, i) * (int)GameMetrics.Current.GridRadius;
                Corners[i] = corner;
            }

            int playerNumber = battlePlayer.GetPlayerNumber() % HexOperations.DirectionsCount;

            if (GameMetrics.Current.PlayerBorder.TryGetValueForKey(playerNumber, out int edge))
            {
                HexCoordinates a = Corners[edge];
                HexCoordinates b = Corners[(edge + 1) % HexOperations.DirectionsCount];
                builder.With(new LinePattern(a, b));
            }
        }
    }
}