using System.Collections.Generic;
using ATCG.Metrics;
using Linework.SurfaceFill;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime.FX
{
    /// <summary>
    /// Same as WideOutlineHighlightController but for Linework SurfaceFill: the theme's Fill SOs are
    /// added to the settings on phase begin and removed when the theme changes / no phase is active.
    /// Each Fill carries its own RenderingLayer, so it drives whichever preview slot it targets.
    /// </summary>
    public class SurfaceFillHighlightController : HighlightThemeController
    {
        [SerializeField]
        private SurfaceFillSettings settings;

        private readonly List<Fill> applied = new();

        protected override void ApplyTheme(HighlightTheme theme)
        {
            if (settings == null || settings.Fills == null)
                return;

            // Remove what the previous theme added (leave everything else, e.g. per-player fills).
            for (int i = 0; i < applied.Count; i++)
                settings.Fills.Remove(applied[i]);
            applied.Clear();

            GameMetrics metrics = GameMetrics.Current;
            if (theme != null && metrics != null)
            {
                foreach ((Fill fill, HighlightState state) in theme.ActiveFills)
                {
                    fill.RenderingLayer = metrics.GetHighlightLayer(state);
                    fill.SetActive(true);
                    settings.Fills.Add(fill);
                    applied.Add(fill);
                }
            }

            settings.Changed();
        }
    }
}
