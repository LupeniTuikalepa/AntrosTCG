using System.Collections.Generic;
using ATCG.Metrics;
using Linework.SurfaceFill;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime.FX
{
    /// <summary>
    /// Same as WideOutlineHighlightController but for Linework SurfaceFill. Adds runtime CLONES of the
    /// theme's active fills (never the shared sub-assets, so nothing gets baked into the settings asset
    /// in Play Mode) and destroys them when the theme changes. Stale null entries are purged on the way in.
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

            settings.Fills.RemoveAll(f => f == null); // purge stale clones from a prior session
            ClearApplied();

            GameMetrics metrics = GameMetrics.Current;
            if (theme != null && metrics != null)
            {
                foreach ((Fill fill, HighlightState state) in theme.ActiveFills)
                {
                    if (fill == null)
                        continue;

                    Fill clone = Instantiate(fill);
                    clone.Cleanup(); // drop the shared material ref so Linework assigns a fresh unique one
                    clone.RenderingLayer = metrics.GetHighlightLayer(state);
                    clone.SetActive(true);
                    settings.Fills.Add(clone);
                    applied.Add(clone);
                }
            }

            settings.Changed();
        }

        private void ClearApplied()
        {
            for (int i = 0; i < applied.Count; i++)
            {
                settings.Fills.Remove(applied[i]);
                if (applied[i] != null)
                    Destroy(applied[i]);
            }

            applied.Clear();
        }
    }
}
