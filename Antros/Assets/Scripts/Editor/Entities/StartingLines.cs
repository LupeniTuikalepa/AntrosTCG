using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Reconstructs the per-player starting lines for orientation on the grid view.
    ///
    /// The game derives a player's starting line from the big hexagon's corners:
    /// corner[i] = neighbor(center, i) * GridRadius, and player N owns the edge from
    /// corner[edge] to corner[edge+1], where edge = PlayerBorder[N]. We reproduce that
    /// for the six player numbers (0..5) so the grid shows where each player starts.
    ///
    /// GameMetrics lives in another assembly, so it's read reflectively and degrades
    /// gracefully: if anything is missing, no lines are produced and the grid still works.
    /// </summary>
    public static class StartingLines
    {
        // Axial directions, matching HexOperations.Directions order.
        private static readonly (int x, int y)[] Directions =
        {
            (1, 0), (1, -1), (0, -1), (-1, 0), (-1, 1), (0, 1)
        };

        public readonly struct Line
        {
            public readonly int PlayerNumber;
            public readonly HexCoordReader.Axial A;
            public readonly HexCoordReader.Axial B;
            public readonly Color Color;

            public Line(int playerNumber, HexCoordReader.Axial a, HexCoordReader.Axial b, Color color)
            {
                PlayerNumber = playerNumber;
                A = a;
                B = b;
                Color = color;
            }
        }

        // Distinct, readable colors per player number.
        private static readonly Color[] PlayerColors =
        {
            new(0.36f, 0.66f, 1.00f), // 0 blue
            new(0.95f, 0.45f, 0.40f), // 1 red
            new(0.45f, 0.85f, 0.55f), // 2 green
            new(0.95f, 0.78f, 0.35f), // 3 amber
            new(0.75f, 0.55f, 0.95f), // 4 purple
            new(0.40f, 0.85f, 0.85f), // 5 teal
        };

        public static List<Line> Compute()
        {
            var lines = new List<Line>();

            if (!TryGetGridRadius(out int radius) || radius <= 0)
                return lines;

            // corner[i] = direction[i] * radius
            var corners = new HexCoordReader.Axial[6];
            for (int i = 0; i < 6; i++)
                corners[i] = new HexCoordReader.Axial(Directions[i].x * radius, Directions[i].y * radius);

            for (int playerNumber = 0; playerNumber < 6; playerNumber++)
            {
                if (!TryGetPlayerBorderEdge(playerNumber, out int edge))
                    continue;

                HexCoordReader.Axial a = corners[edge % 6];
                HexCoordReader.Axial b = corners[(edge + 1) % 6];
                lines.Add(new Line(playerNumber, a, b, PlayerColors[playerNumber % PlayerColors.Length]));
            }

            return lines;
        }

        // ---- reflective access to GameMetrics.Current ----

        private static object metricsCache;
        private static bool metricsResolved;

        private static object GetMetrics()
        {
            if (metricsResolved)
                return metricsCache;
            metricsResolved = true;

            Type gm = FindType("ATCG.Metrics.GameMetrics");
            if (gm == null)
                return null;

            // GameSettings<T>.Current — a static property on the base or the type.
            PropertyInfo current = gm.GetProperty("Current",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (current == null)
                return null;

            try { metricsCache = current.GetValue(null); }
            catch { metricsCache = null; }
            return metricsCache;
        }

        private static bool TryGetGridRadius(out int radius)
        {
            radius = 0;
            object metrics = GetMetrics();
            if (metrics == null)
                return false;

            PropertyInfo prop = metrics.GetType().GetProperty("GridRadius",
                BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return false;

            try
            {
                object v = prop.GetValue(metrics);
                radius = Convert.ToInt32(v);
                return true;
            }
            catch { return false; }
        }

        private static bool TryGetPlayerBorderEdge(int playerNumber, out int edge)
        {
            edge = 0;
            object metrics = GetMetrics();
            if (metrics == null)
                return false;

            PropertyInfo borderProp = metrics.GetType().GetProperty("PlayerBorder",
                BindingFlags.Public | BindingFlags.Instance);
            if (borderProp == null)
                return false;

            object border;
            try { border = borderProp.GetValue(metrics); }
            catch { return false; }
            if (border == null)
                return false;

            // DualPairing<int,int>.TryGetValueForKey(int key, out int value)
            MethodInfo tryGet = border.GetType().GetMethod("TryGetValueForKey",
                BindingFlags.Public | BindingFlags.Instance);
            if (tryGet == null)
                return false;

            object[] args = { playerNumber % 6, 0 };
            try
            {
                object ok = tryGet.Invoke(border, args);
                if (ok is true)
                {
                    edge = Convert.ToInt32(args[1]);
                    return true;
                }
            }
            catch { /* fall through */ }
            return false;
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
