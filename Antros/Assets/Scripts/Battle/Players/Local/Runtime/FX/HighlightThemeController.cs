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
    /// The Linework-specific application (writing colours onto the outline settings) is done in
    /// <see cref="ApplyTheme"/>. Override it in a subclass bound to your outline type — see the
    /// completion notes for the exact WideOutlineSettings pattern.
    /// </summary>
    public class HighlightThemeController : LocalPlayerMonoPhaseListener<IHighlightingPhase>
    {
        private Priority<HighlightTheme> activeTheme;
        private int depthCounter;

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

            ApplyTheme(ActiveTheme.Value);
        }

        protected override void OnPhaseEnd(IHighlightingPhase phase)
        {
            ActiveTheme.RemovePriority(phase.HighlightChannel);
            ApplyTheme(ActiveTheme.Value);
            base.OnPhaseEnd(phase);
        }

        /// <summary>
        /// Apply the winning theme to the PREVIEW outline entries only — every other state keeps the
        /// Linework default look, so only touch outlines mapped to a preview state
        /// (GameMetrics.Current.GetHighlightLayer(Preview1..4)): set color + SetActive from the theme
        /// entry, then settings.Changed(). A null <paramref name="theme"/> means no phase is active
        /// → turn the preview outlines off, leave the rest untouched.
        /// </summary>
        protected virtual void ApplyTheme(HighlightTheme theme)
        {
        }
    }
}
