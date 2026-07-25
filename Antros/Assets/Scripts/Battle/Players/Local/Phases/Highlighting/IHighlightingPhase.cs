using ATCG.Metrics;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.Phases
{
    /// <summary>
    /// A phase that contributes a highlight theme (the colours used per <see cref="HighlightState"/>)
    /// while it is active. The HighlightThemeController layers contributions by priority through a
    /// Priority&lt;HighlightTheme&gt;, so a nested phase's theme wins and the parent's resurfaces on
    /// close. The phase's own ChannelKey identifies its contribution.
    /// </summary>
    public interface IHighlightingPhase : IPhase, ILocalPlayerPhase
    {
        ChannelKey HighlightChannel { get; }
        HighlightTheme HighlightTheme { get; }
    }
}
