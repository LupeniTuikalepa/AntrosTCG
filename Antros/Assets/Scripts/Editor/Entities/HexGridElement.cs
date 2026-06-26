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
        private List<StartingLines.Line> startingLines = new();

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

        public void SetStartingLines(List<StartingLines.Line> lines)
        {
            startingLines = lines ?? new List<StartingLines.Line>();
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
            if (cells.Count == 0 && startingLines.Count == 0)
                return;

            float w = Mathf.Sqrt(3f) * hexSize;
            float h = 1.5f * hexSize;
            projW = w;
            projH = h;

            // Bounds over cells AND line endpoints, so nothing is clipped.
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
            foreach (StartingLines.Line line in startingLines)
            {
                Accumulate(line.A);
                Accumulate(line.B);
            }

            float gridW = (maxX - minX) + w;
            float gridH = (maxY - minY) + h;
            projOffX = (content.width - gridW) * 0.5f - minX + w * 0.5f;
            projOffY = (content.height - gridH) * 0.5f - minY + h * 0.5f;

            Painter2D p = ctx.painter2D;

            // Cells first.
            foreach (KeyValuePair<HexCoordReader.Axial, List<int>> kv in cells)
            {
                Vector2 c = Project(kv.Key);
                centers[kv.Key] = c;

                bool isSel = selected != null && selected.Value.Equals(kv.Key);
                DrawHex(p, c, hexSize, isSel, kv.Value.Count);
            }

            // Starting lines on top, one colored segment per player.
            foreach (StartingLines.Line line in startingLines)
            {
                Vector2 a = Project(line.A);
                Vector2 b = Project(line.B);
                p.strokeColor = line.Color;
                p.lineWidth = 3f;
                p.BeginPath();
                p.MoveTo(a);
                p.LineTo(b);
                p.Stroke();

                // small player marker at the midpoint (diamond, avoids Arc/Angle API)
                Vector2 mid = (a + b) * 0.5f;
                float r = 5f;
                p.fillColor = line.Color;
                p.BeginPath();
                p.MoveTo(new Vector2(mid.x, mid.y - r));
                p.LineTo(new Vector2(mid.x + r, mid.y));
                p.LineTo(new Vector2(mid.x, mid.y + r));
                p.LineTo(new Vector2(mid.x - r, mid.y));
                p.ClosePath();
                p.Fill();
            }
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