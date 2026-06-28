using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Surfaces anomalies from the inspector without flooding the console. The window
    /// rebuilds up to several times a second, so the same fault (e.g. a store missing
    /// for a registered id, a mapping that drifted between plays) would otherwise log
    /// hundreds of times. Each distinct message is logged at most once per interval.
    ///
    /// This exists because silent catches in a debug tool hide the very problems the
    /// tool is meant to reveal — notably the ComponentRegistry id drift across plays.
    /// </summary>
    public static class InspectorLog
    {
        private const double ThrottleSeconds = 2.0;

        private static readonly Dictionary<string, double> lastLogged = new();

        public static void Warn(string message)
        {
            double now = Time.realtimeSinceStartupAsDouble;
            if (lastLogged.TryGetValue(message, out double t) && now - t < ThrottleSeconds)
                return;

            lastLogged[message] = now;
            Debug.LogWarning($"[World Inspector] {message}");
        }

        public static void Warn(string context, System.Exception e)
        {
            Warn($"{context}: {e.GetType().Name} — {e.Message}");
        }
    }
}