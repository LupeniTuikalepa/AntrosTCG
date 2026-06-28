using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Reconstructs each player's starting line as colored hex-cell edges, matching the
    /// in-game look (the border of the cells, not a straight segment).
    ///
    /// A player owns the edge of the big hexagon from corner[edge] to corner[edge+1]
    /// (edge = PlayerBorder[playerNumber], corner[i] = direction[i] * GridRadius). We walk
    /// the cells along that line and, for each, color the OUTER edges — those facing a
    /// neighbor outside the grid (distance > radius). On a shared corner cell, edges facing
    /// out on player A's side get A's color and those on B's side get B's, automatically.
    ///
    /// GameMetrics lives in another assembly, read reflectively; degrades to no edges.
    /// </summary>
    public static class StartingLines
    {
        // Axial directions, matching HexOperations.Directions order. Edge i of a hex
        // faces neighbor (x+dir[i].x, y+dir[i].y).
        private static readonly (int x, int y)[] Directions =
        {
            (1, 0), (1, -1), (0, -1), (-1, 0), (-1, 1), (0, 1)
        };

        private static readonly Color[] PlayerColors =
        {
            new(0.36f, 0.66f, 1.00f), // 0 blue
            new(0.95f, 0.45f, 0.40f), // 1 red
            new(0.45f, 0.85f, 0.55f), // 2 green
            new(0.95f, 0.78f, 0.35f), // 3 amber
            new(0.75f, 0.55f, 0.95f), // 4 purple
            new(0.40f, 0.85f, 0.85f), // 5 teal
        };

        /// <summary>One colored edge of one cell. DirectionIndex is the neighbor direction
        /// the edge faces (0..5), used by the renderer to pick the two hex vertices.</summary>
        public readonly struct Edge
        {
            public readonly HexCoordReader.Axial Cell;
            public readonly int DirectionIndex;
            public readonly Color Color;

            public Edge(HexCoordReader.Axial cell, int directionIndex, Color color)
            {
                Cell = cell;
                DirectionIndex = directionIndex;
                Color = color;
            }
        }

        public static List<Edge> ComputeEdges()
        {
            var edges = new List<Edge>();

            if (!TryGetGridRadius(out int radius) || radius <= 0)
            {
                InspectorLog.Warn("Starting lines unavailable: couldn't read GameMetrics.Current.GridRadius (metrics assembly not referenced or not initialized).");
                return edges;
            }

            var corners = new HexCoordReader.Axial[6];
            for (int i = 0; i < 6; i++)
                corners[i] = new HexCoordReader.Axial(Directions[i].x * radius, Directions[i].y * radius);

            // Gather, per player, the cells on their starting line and their line's
            // outward direction (used to break ties on shared corner cells).
            var lineCellsByPlayer = new Dictionary<int, HashSet<HexCoordReader.Axial>>();
            var lineDirByPlayer = new Dictionary<int, Vector2>();

            for (int playerNumber = 0; playerNumber < 6; playerNumber++)
            {
                if (!TryGetPlayerBorderEdge(playerNumber, out int edge))
                    continue;

                HexCoordReader.Axial a = corners[edge % 6];
                HexCoordReader.Axial b = corners[(edge + 1) % 6];

                var set = new HashSet<HexCoordReader.Axial>();
                foreach (HexCoordReader.Axial cell in Line(a, b))
                    set.Add(cell);
                lineCellsByPlayer[playerNumber] = set;

                // Outward direction of this player's border = average pixel direction of
                // the two corners (points away from center). Used to resolve corners.
                Vector2 dir = (PixelDir(a) + PixelDir(b)).normalized;
                lineDirByPlayer[playerNumber] = dir;
            }

            // For every outer edge of every line cell, assign it to exactly ONE player:
            // the one whose line owns that side. On a shared corner cell, each outer
            // edge goes to the player whose outward line direction best matches the
            // edge's own outward normal — so the corner splits coherently with the
            // colors of the adjacent (exclusive) cells.
            var assigned = new HashSet<(int x, int y, int dir)>();

            foreach (KeyValuePair<int, HashSet<HexCoordReader.Axial>> kv in lineCellsByPlayer)
            {
                int playerNumber = kv.Key;
                foreach (HexCoordReader.Axial cell in kv.Value)
                {
                    // Outline the whole cell: all six sides get the player's color, not
                    // only the ones facing out of the grid.
                    for (int d = 0; d < 6; d++)
                    {
                        var key = (cell.X, cell.Y, d);
                        if (assigned.Contains(key))
                            continue;

                        int owner = ResolveOwner(cell, d, lineCellsByPlayer, lineDirByPlayer);
                        if (owner < 0)
                            owner = playerNumber;

                        assigned.Add(key);
                        edges.Add(new Edge(cell, d, PlayerColors[owner % PlayerColors.Length]));
                    }
                }
            }

            return edges;
        }

        /// <summary>
        /// Which player owns a given outer edge. If only one player's line includes the
        /// cell, that's the owner. If several do (a shared corner cell), pick the player
        /// whose outward line direction is closest to this edge's outward normal.
        /// </summary>
        private static int ResolveOwner(
            HexCoordReader.Axial cell, int dir,
            Dictionary<int, HashSet<HexCoordReader.Axial>> lineCellsByPlayer,
            Dictionary<int, Vector2> lineDirByPlayer)
        {
            int single = -1;
            int owners = 0;
            foreach (KeyValuePair<int, HashSet<HexCoordReader.Axial>> kv in lineCellsByPlayer)
            {
                if (kv.Value.Contains(cell))
                {
                    owners++;
                    single = kv.Key;
                }
            }

            if (owners <= 1)
                return single;

            // Shared corner: compare the edge's outward normal to each candidate's dir.
            Vector2 edgeNormal = new(Directions[dir].x + Directions[dir].y * 0.5f, -Directions[dir].y);
            edgeNormal = edgeNormal.normalized;

            int best = -1;
            float bestDot = float.MinValue;
            foreach (KeyValuePair<int, HashSet<HexCoordReader.Axial>> kv in lineCellsByPlayer)
            {
                if (!kv.Value.Contains(cell))
                    continue;
                float dot = Vector2.Dot(edgeNormal, lineDirByPlayer[kv.Key]);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = kv.Key;
                }
            }
            return best;
        }

        private static Vector2 PixelDir(HexCoordReader.Axial a)
        {
            // Pointy-top pixel position (matches the renderer's projection), normalized.
            Vector2 v = new(a.X + a.Y * 0.5f, -a.Y);
            return v.sqrMagnitude > 0.0001f ? v.normalized : v;
        }

        // ---- hex math (cube distance and line, mirrors HexOperations) ----

        private static int Distance(HexCoordReader.Axial c)
            => (Mathf.Abs(c.X) + Mathf.Abs(c.Y) + Mathf.Abs(c.Z)) / 2;

        private static IEnumerable<HexCoordReader.Axial> Line(HexCoordReader.Axial a, HexCoordReader.Axial b)
        {
            int n = Distance(new HexCoordReader.Axial(a.X - b.X, a.Y - b.Y));
            n = Mathf.Max(n, 1);

            for (int i = 0; i <= n; i++)
            {
                float t = (float)i / n;
                float fx = Mathf.Lerp(a.X, b.X, t);
                float fy = Mathf.Lerp(a.Y, b.Y, t);
                float fz = Mathf.Lerp(a.Z, b.Z, t);
                yield return CubeRound(fx, fy, fz);
            }
        }

        private static HexCoordReader.Axial CubeRound(float x, float y, float z)
        {
            int rx = Mathf.RoundToInt(x);
            int ry = Mathf.RoundToInt(y);
            int rz = Mathf.RoundToInt(z);

            float dx = Mathf.Abs(rx - x);
            float dy = Mathf.Abs(ry - y);
            float dz = Mathf.Abs(rz - z);

            if (dx > dy && dx > dz) rx = -ry - rz;
            else if (dy > dz) ry = -rx - rz;
            // z is derived in Axial, so we only need x and y.
            return new HexCoordReader.Axial(rx, ry);
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

            try { radius = Convert.ToInt32(prop.GetValue(metrics)); return true; }
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
