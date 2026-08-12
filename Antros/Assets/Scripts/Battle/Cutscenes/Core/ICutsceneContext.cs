namespace ATCG.Cutscenes
{
    /// <summary>
    /// The driving surface a cutscene needs, independent of what kind of thing plays it (a
    /// capacity, a physical attack, a passive activation, a card arrival…). It is a keyed,
    /// typed property bag: cutscene elements bind to this instead of the concrete phase, so the
    /// same elements and the same editor preview work across every cutscene-driven system.
    /// Roles (source/target actors, coordinates) are layered on top of this in a later step.
    /// </summary>
    public interface ICutsceneContext
    {
        bool TryGetProperty<T>(string name, out T value);
        void InjectProperty<T>(string name, T value);
    }
}
