using System.Collections.Generic;
using ATCG.Metrics;
using Linework.WideOutline;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime.FX
{
    /// <summary>
    /// Pushes the active phase's theme onto a Linework WideOutline settings: the theme's Outline SOs
    /// are added to the settings on phase begin and removed when the theme changes / no phase is
    /// active. Each Outline carries its own RenderingLayer, so it lights up whichever preview slot it
    /// targets — no mapping here.
    /// </summary>
    public class WideOutlineHighlightController : HighlightThemeController
    {
        [SerializeField]
        private WideOutlineSettings settings;

        private readonly List<Outline> applied = new();

        protected override void ApplyTheme(HighlightTheme theme)
        {
            if (settings == null || settings.Outlines == null)
                return;

            // Remove what the previous theme added (leave everything else, e.g. Selected/Hovered).
            for (int i = 0; i < applied.Count; i++)
                settings.Outlines.Remove(applied[i]);
            applied.Clear();

            GameMetrics metrics = GameMetrics.Current;
            if (theme != null && metrics != null)
            {
                foreach ((Outline outline, HighlightState state) in theme.ActiveOutlines)
                {
                    outline.RenderingLayer = metrics.GetHighlightLayer(state);
                    outline.SetActive(true);
                    settings.Outlines.Add(outline);
                    applied.Add(outline);
                }
            }

            settings.Changed();
        }
    }
}
