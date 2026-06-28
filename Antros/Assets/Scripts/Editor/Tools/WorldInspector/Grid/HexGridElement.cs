using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Draws a pointy-top hex grid from a set of occupied axial cells and hit-tests
    /// pointer clicks back to a cell. Pure presentation: it knows nothing about the
    /// world, only the cell -> entity-count map handed to it.
    /// </summary>
    public sealed class HexGridElement : VisualElement
    {
        private readonly Action<HexCoordReader.Axial> onCellClicked;

        private Dictionary<HexCoordReader.Axial, List<int>> cells = new();
        private HexCoordReader.Axial? selected;
        private List<StartingLines.Edge> startingEdges = new();
        private HashSet<HexCoordReader.Axial> markedCells = new();

        // Layout cache, rebuilt on draw: pixel center per cell.
        private readonly Dictionary<HexCoordReader.Axial, Vector2> centers = new();
        private float hexSize = 26f;

        // Projection state (set during draw) so lines use the same mapping as cells.
        private float projW, projH, projOffX, projOffY;

        public HexGridElement(Action<HexCoordReader.Axial> onCellClicked)
        {
            this.onCellClicked = onCellClicked;
            generateVisualContent += OnGenerate;
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());
        }

        public void SetCells(Dictionary<HexCoordReader.Axial, List<int>> cells, HexCoordReader.Axial? selected)
        {
            this.cells = cells ?? new Dictionary<HexCoordReader.Axial, List<int>>();
            this.selected = selected;
            MarkDirtyRepaint();
        }

        public void SetStartingEdges(List<StartingLines.Edge> edges)
        {
            startingEdges = edges ?? new List<StartingLines.Edge>();
            MarkDirtyRepaint();
        }

        public void SetMarkedCells(HashSet<HexCoordReader.Axial> marked)
        {
            markedCells = marked ?? new HashSet<HexCoordReader.Axial>();
            MarkDirtyRepaint();
        }

        public void SetSelected(HexCoordReader.Axial? cell)
        {
            selected = cell;
            MarkDirtyRepaint();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            Vector2 p = evt.localPosition;
            HexCoordReader.Axial? hit = null;
            float best = float.MaxValue;

            // Nearest-center hit test, accepted only within the hex radius.
            foreach (KeyValuePair<HexCoordReader.Axial, Vector2> kv in centers)
            {
                float d = (kv.Value - p).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    hit = kv.Key;
                }
            }

            if (hit != null && best <= hexSize * hexSize)
                onCellClicked?.Invoke(hit.Value);
        }

        private void OnGenerate(MeshGenerationContext ctx)
        {
            centers.Clear();

            Rect content = contentRect;
            if (content.width < 4 || content.height < 4)
                return;
            if (cells.Count == 0 && startingEdges.Count == 0)
                return;

            float w = Mathf.Sqrt(3f) * hexSize;
            float h = 1.5f * hexSize;
            projW = w;
            projH = h;

            // Bounds over cells AND starting-edge cells, so nothing is clipped.
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;

            void Accumulate(HexCoordReader.Axial a)
            {
                float px = w * (a.X + a.Y * 0.5f);
                float py = -h * a.Y;
                minX = Mathf.Min(minX, px); maxX = Mathf.Max(maxX, px);
                minY = Mathf.Min(minY, py); maxY = Mathf.Max(maxY, py);
            }

            foreach (HexCoordReader.Axial a in cells.Keys)
                Accumulate(a);
            foreach (StartingLines.Edge e in startingEdges)
                Accumulate(e.Cell);

            float gridW = (maxX - minX) + w;
            float gridH = (maxY - minY) + h;
            projOffX = (content.width - gridW) * 0.5f - minX + w * 0.5f;
            projOffY = (content.height - gridH) * 0.5f - minY + h * 0.5f;

            Painter2D p = ctx.painter2D;

            // 1) Cells (fill + default outline).
            foreach (KeyValuePair<HexCoordReader.Axial, List<int>> kv in cells)
            {
                Vector2 c = Project(kv.Key);
                centers[kv.Key] = c;

                bool isSel = selected != null && selected.Value.Equals(kv.Key);
                DrawHex(p, c, hexSize, isSel, kv.Value.Count);
            }

            // 2) Player starting edges: the outer border of each line cell, colored.
            foreach (StartingLines.Edge e in startingEdges)
            {
                Vector2 c = Project(e.Cell);
                GetEdgeEndpoints(c, hexSize, e.DirectionIndex, out Vector2 v0, out Vector2 v1);
                p.strokeColor = e.Color;
                p.lineWidth = 3f;
                p.BeginPath();
                p.MoveTo(v0);
                p.LineTo(v1);
                p.Stroke();
            }

            // 3) Aspect markers last, so they sit on top of the colored edges.
            foreach (KeyValuePair<HexCoordReader.Axial, List<int>> kv in cells)
            {
                if (!markedCells.Contains(kv.Key))
                    continue;
                DrawAspectMarker(p, Project(kv.Key), hexSize);
            }
        }

        // Axial neighbor directions, same order as elsewhere.
        private static readonly (int x, int y)[] NeighborDirs =
        {
            (1, 0), (1, -1), (0, -1), (-1, 0), (-1, 1), (0, 1)
        };

        /// <summary>
        /// The two vertices of the hex side facing neighbor direction `dir`. Computed
        /// geometrically: the shared side is the pair of adjacent vertices whose midpoint
        /// points toward the neighbor — no hardcoded vertex/direction mapping to get wrong.
        /// </summary>
        private void GetEdgeEndpoints(Vector2 center, float size, int dir, out Vector2 v0, out Vector2 v1)
        {
            // Neighbor's pixel offset (same projection as the grid), as a direction.
            var d = NeighborDirs[dir % 6];
            Vector2 toNeighbor = new(projW * (d.x + d.y * 0.5f), -projH * d.y);
            if (toNeighbor.sqrMagnitude > 0.0001f)
                toNeighbor = toNeighbor.normalized;

            // Pick the adjacent vertex pair whose midpoint direction best matches.
            int bestK = 0;
            float bestDot = float.MinValue;
            for (int k = 0; k < 6; k++)
            {
                Vector2 a = Vertex(center, size, k);
                Vector2 b = Vertex(center, size, (k + 1) % 6);
                Vector2 mid = ((a + b) * 0.5f - center);
                if (mid.sqrMagnitude > 0.0001f)
                    mid = mid.normalized;
                float dot = Vector2.Dot(mid, toNeighbor);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestK = k;
                }
            }

            v0 = Vertex(center, size, bestK);
            v1 = Vertex(center, size, (bestK + 1) % 6);
        }

        private static Vector2 Vertex(Vector2 center, float size, int k)
        {
            float ang = Mathf.Deg2Rad * (60f * k - 90f);
            return new Vector2(center.x + size * Mathf.Cos(ang), center.y + size * Mathf.Sin(ang));
        }

        private static void DrawAspectMarker(Painter2D p, Vector2 center, float size)
        {
            // A small 4-point star to mark "an entity here matches a checked aspect".
            float outer = size * 0.42f;
            float inner = size * 0.17f;
            p.fillColor = new Color(0.98f, 0.85f, 0.40f);
            p.strokeColor = new Color(0.20f, 0.18f, 0.10f);
            p.lineWidth = 1f;
            p.BeginPath();
            for (int i = 0; i < 8; i++)
            {
                float r = (i % 2 == 0) ? outer : inner;
                float ang = Mathf.Deg2Rad * (45f * i - 90f);
                Vector2 pt = new(center.x + r * Mathf.Cos(ang), center.y + r * Mathf.Sin(ang));
                if (i == 0) p.MoveTo(pt);
                else p.LineTo(pt);
            }
            p.ClosePath();
            p.Fill();
            p.Stroke();
        }

        private Vector2 Project(HexCoordReader.Axial a)
        {
            float px = projW * (a.X + a.Y * 0.5f);
            float py = -projH * a.Y;
            return new Vector2(px + projOffX, py + projOffY);
        }

        private static void DrawHex(Painter2D p, Vector2 center, float size, bool selected, int count)
        {
            // Pointy-top: vertices at 30, 90, 150, 210, 270, 330 degrees.
            Span<Vector2> pts = stackalloc Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (60f * i - 90f);
                pts[i] = new Vector2(center.x + size * Mathf.Cos(angle), center.y + size * Mathf.Sin(angle));
            }

            Color fill = selected
                ? new Color(0.20f, 0.36f, 0.55f)
                : count > 0 ? new Color(0.26f, 0.27f, 0.30f) : new Color(0.20f, 0.20f, 0.22f);
            Color stroke = selected ? new Color(0.36f, 0.66f, 1f) : new Color(0.40f, 0.40f, 0.44f);

            p.fillColor = fill;
            p.strokeColor = stroke;
            p.lineWidth = selected ? 2f : 1f;

            p.BeginPath();
            p.MoveTo(pts[0]);
            for (int i = 1; i < 6; i++)
                p.LineTo(pts[i]);
            p.ClosePath();
            p.Fill();
            p.Stroke();
        }
    }
}
