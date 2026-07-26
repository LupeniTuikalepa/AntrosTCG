using ATCG.Battle.Players.Local.Phases;
using ATCG.Metrics;
using Helteix.ChanneledProperties.Priorities;

namespace ATCG.Battle.Players.Local.Runtime.FX
{
    /// <summary>
    /// Scene object that swaps the active highlight look per phase. Each highlighting phase pushes
    /// its <see cref="HighlightTheme"/> onto a Priority&lt;HighlightTheme&gt; keyed by the phase's
    /// channel; the highest-priority (most recently opened) theme wins, and closing a phase pops
    /// its contribution so the parent's theme resurfaces.
    ///
    /// The Linework-specific application is done in <see cref="ApplyTheme"/>, overridden per settings
    /// type (WideOutlineHighlightController / SurfaceFillHighlightController) to add clones of the
    /// theme's active entries to the settings and remove the previous ones.
    /// </summary>
    public class HighlightThemeController : LocalPlayerMonoPhaseListener<IHighlightingPhase>
    {
        private Priority<HighlightTheme> activeTheme;
        private int depthCounter;
        private HighlightTheme lastApplied;
        private bool hasApplied;

        // Lazy: build the Priority on first use (in play mode) rather than in a field initializer,
        // which runs at MonoBehaviour construction — too early, ChanneledPropertiesSettings.Current
        // isn't ready yet and the ctor would throw, leaving the field null.
        private Priority<HighlightTheme> ActiveTheme
            => activeTheme ??= new Priority<HighlightTheme>(defaultValue: null, capacity: 16, expandWhenFull: true);

        protected override void OnPhaseBegin(IHighlightingPhase phase)
        {
            base.OnPhaseBegin(phase);

            if (phase.HighlightTheme != null)
                // ++depthCounter → the most recently opened phase always wins; RemovePriority on
                // close makes the previous one resurface (or null → previews off).
                ActiveTheme.AddPriority(phase.HighlightChannel, ++depthCounter, phase.HighlightTheme);

            ApplyIfChanged();
        }

        protected override void OnPhaseEnd(IHighlightingPhase phase)
        {
            ActiveTheme.RemovePriority(phase.HighlightChannel);
            ApplyIfChanged();
            base.OnPhaseEnd(phase);
        }

        // Every selection phase now flows through here; only re-apply (re-clone) when the winning theme
        // actually changes, to avoid churn during phases that don't contribute a theme.
        private void ApplyIfChanged()
        {
            HighlightTheme value = ActiveTheme.Value;
            if (hasApplied && value == lastApplied)
                return;

            lastApplied = value;
            hasApplied = true;
            ApplyTheme(value);
        }

        /// <summary>
        /// Apply the winning theme to the Linework settings. Overridden per Linework type (WideOutline /
        /// SurfaceFill) to add clones of the theme's active entries and remove the previous ones. A null
        /// <paramref name="theme"/> means no phase is active → remove everything the controller added.
        /// </summary>
        protected virtual void ApplyTheme(HighlightTheme theme)
        {
        }
    }
}
