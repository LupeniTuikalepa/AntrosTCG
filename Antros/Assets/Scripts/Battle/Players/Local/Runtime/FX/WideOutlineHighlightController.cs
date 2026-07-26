using System.Collections.Generic;
using ATCG.Metrics;
using Linework.WideOutline;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Runtime.FX
{
    /// <summary>
    /// Pushes the active phase's theme onto a Linework WideOutline settings. To avoid mutating the
    /// shared theme sub-assets (and persisting them into the settings asset in Play Mode), it adds
    /// runtime CLONES of the theme's active outlines and destroys them when the theme changes. Stale
    /// null entries left by a previous session are purged on the way in.
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

            settings.Outlines.RemoveAll(o => o == null); // purge stale clones from a prior session
            ClearApplied();

            GameMetrics metrics = GameMetrics.Current;
            if (theme != null && metrics != null)
            {
                foreach ((Outline outline, HighlightState state) in theme.ActiveOutlines)
                {
                    if (outline == null)
                        continue;

                    Outline clone = Instantiate(outline);
                    clone.Cleanup(); // drop the shared material refs so Linework assigns fresh unique ones
                    clone.RenderingLayer = metrics.GetHighlightLayer(state);
                    clone.SetActive(true);
                    settings.Outlines.Add(clone);
                    applied.Add(clone);
                }
            }

            settings.Changed();
        }

        private void ClearApplied()
        {
            for (int i = 0; i < applied.Count; i++)
            {
                settings.Outlines.Remove(applied[i]);
                if (applied[i] != null)
                    Destroy(applied[i]);
            }

            applied.Clear();
        }
    }
}
