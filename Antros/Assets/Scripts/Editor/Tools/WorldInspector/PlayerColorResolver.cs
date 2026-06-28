using System;
using System.Reflection;
using UnityEngine;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Maps a player number to a color, preferring the game's own palette
    /// (GameMetrics.GetPlayerColor) and falling back to a stable derived hue if the
    /// metrics assembly can't be reached. Entities with no player get white.
    /// </summary>
    public static class PlayerColorResolver
    {
        private const int PlayerCount = 6;

        public static readonly Color NoPlayer = Color.white;

        private static object metricsCache;
        private static MethodInfo getPlayerColor;
        private static bool resolved;

        public static Color ForPlayerNumber(int playerNumber)
        {
            if (TryGameMetricsColor(playerNumber, out Color c))
                return c;
            return DerivedColor(playerNumber);
        }

        private static bool TryGameMetricsColor(int playerNumber, out Color color)
        {
            color = default;
            if (!ResolveMetrics())
                return false;

            try
            {
                object result = getPlayerColor.Invoke(metricsCache, new object[] { playerNumber, PlayerCount });
                if (result is Color c)
                {
                    color = c;
                    return true;
                }
            }
            catch (Exception e)
            {
                InspectorLog.Warn("GameMetrics.GetPlayerColor threw; using derived color", e);
            }
            return false;
        }

        private static bool ResolveMetrics()
        {
            if (resolved)
                return metricsCache != null && getPlayerColor != null;
            resolved = true;

            Type gm = FindType("ATCG.Metrics.GameMetrics");
            if (gm == null)
                return false;

            PropertyInfo current = gm.GetProperty("Current",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (current == null)
                return false;

            try { metricsCache = current.GetValue(null); }
            catch { metricsCache = null; }
            if (metricsCache == null)
                return false;

            getPlayerColor = gm.GetMethod("GetPlayerColor", BindingFlags.Public | BindingFlags.Instance);
            return getPlayerColor != null;
        }

        // Deterministic hue from the player number so colors are stable per player.
        private static Color DerivedColor(int playerNumber)
        {
            float hue = (playerNumber * 0.137f) % 1f;
            return Color.HSVToRGB(hue, 0.55f, 0.95f);
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType(fullName);
                if (t != null)
                    return t;
            }
            return null;
        }
    }
}
