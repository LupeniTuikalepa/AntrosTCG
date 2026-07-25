namespace ATCG.Metrics
{
    /// <summary>
    /// Visual highlight slot for an entity during a selection / preview phase. Everything a phase
    /// classifies goes through the Preview slots (Preview1..6), coloured per phase via a
    /// HighlightTheme. Hovered/Selected are driven by the selection system, not the phase listener.
    ///
    /// Convention used by the code (tweak the Linework colours per theme):
    ///   Preview1 = movement direct ring   Preview4 = related
    ///   Preview2 = movement reachable      Preview5 = inaccessible / non-selectable
    ///   Preview3 = selectable              Preview6 = spare
    /// </summary>
    public enum HighlightState
    {
        None = 0,
        Hovered,
        Selected,
        Preview1,
        Preview2,
        Preview3,
        Preview4,
        Preview5,
        Preview6,
    }
}
