namespace ATCG.Cutscenes
{
    /// <summary>
    /// A component on a cutscene prefab that binds itself to the driving context when the cutscene
    /// plays (game phase or editor preview) and releases it on teardown. Elements pull whatever
    /// refs/properties they need from the <see cref="ICutsceneContext"/> — source actor, screen
    /// player, injected values — instead of depending on any concrete phase, so the same elements
    /// work across every cutscene-driven system.
    /// </summary>
    public interface ICutsceneElement
    {
        void Connect(ICutsceneContext context);

        void Disconnect();
    }
}
