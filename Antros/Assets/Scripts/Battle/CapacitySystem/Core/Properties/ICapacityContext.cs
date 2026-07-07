namespace ATCG.Battle.CapacitySystem.Core.Properties
{
    /// <summary>
    /// The property surface a cutscene needs from its driving context. Implemented by
    /// CastCapacityPhase in game and by a debug context in the editor preview, so
    /// cutscene elements bind to this instead of the concrete phase. Properties are
    /// keyed by string (see CapacityContextKeys for the well-known ones) and typed at
    /// retrieval — casting stays internal to the property system, not the callers.
    /// </summary>
    public interface ICapacityContext
    {
        bool TryGetProperty<T>(string name, out T value);
        void InjectProperty<T>(string name, T value);
    }
}