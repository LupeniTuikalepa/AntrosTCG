namespace ATCG.Cutscenes
{
    /// <summary>
    /// The generic <see cref="ICutsceneContext"/> a consumer fills before playing a cutscene:
    /// inject the well-known keys the elements expect (source actor, screen player, per-run values)
    /// and any custom values. Backed by an open <see cref="CutscenePropertyBag"/>. The capacity
    /// system keeps its own richer context (schema + live phase delegation); this is the lightweight
    /// one used by the standalone player for attacks / passives / arrivals.
    /// </summary>
    public sealed class CutsceneContext : ICutsceneContext
    {
        private readonly CutscenePropertyBag bag = new();

        public bool TryGetProperty<T>(string name, out T value) => bag.TryGet(name, out value);

        public void InjectProperty<T>(string name, T value) => bag.Set(name, value);

        /// <summary>Fluent inject, so a consumer can build a context inline.</summary>
        public CutsceneContext With<T>(string name, T value)
        {
            bag.Set(name, value);
            return this;
        }
    }
}
