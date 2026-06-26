using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Editor.Commands
{
    /// <summary>
    /// Transparent overlay that draws parent -> child arrows between command cards.
    /// It fills the group body; cards register themselves so the overlay can read
    /// their layout rectangles (resolved after geometry) and stroke a connector
    /// from each parent's right edge to each child's left edge, with an arrowhead.
    /// </summary>
    public sealed class ConnectorLayer : VisualElement
    {
        private readonly List<(VisualElement parent, VisualElement child)> links = new();
        public Color LineColor = new(0.45f, 0.55f, 0.72f);

        public ConnectorLayer()
        {
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;
            generateVisualContent += OnGenerate;
        }

        public void Clear()
        {
            links.Clear();
            MarkDirtyRepaint();
        }

        public void AddLink(VisualElement parent, VisualElement child)
        {
            links.Add((parent, child));
        }

        public void Refresh() => MarkDirtyRepaint();

        private void OnGenerate(MeshGenerationContext ctx)
        {
            Painter2D p = ctx.painter2D;
            p.strokeColor = LineColor;
            p.fillColor = LineColor;
            p.lineWidth = 1.5f;

            foreach ((VisualElement parent, VisualElement child) in links)
            {
                if (parent == null || child == null)
                    continue;

                Rect pr = this.WorldToLocal(parent.worldBound);
                Rect cr = this.WorldToLocal(child.worldBound);

                Vector2 start = new(pr.xMax, pr.center.y);
                Vector2 end = new(cr.xMin, cr.center.y);

                // elbow: out from parent, vertical to child's row, into child
                float midX = (start.x + end.x) * 0.5f;
                p.BeginPath();
                p.MoveTo(start);
                p.LineTo(new Vector2(midX, start.y));
                p.LineTo(new Vector2(midX, end.y));
                p.LineTo(new Vector2(end.x - 6f, end.y));
                p.Stroke();

                // arrowhead
                Vector2 tip = new(end.x, end.y);
                p.BeginPath();
                p.MoveTo(tip);
                p.LineTo(new Vector2(tip.x - 6f, tip.y - 4f));
                p.LineTo(new Vector2(tip.x - 6f, tip.y + 4f));
                p.ClosePath();
                p.Fill();
            }
        }
    }
}