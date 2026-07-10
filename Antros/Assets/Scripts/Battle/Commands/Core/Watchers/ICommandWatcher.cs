namespace ATCG.Battle.Commands.Watchers
{
    public interface ICommandWatcher { }

    /// <summary>
    /// Like listeners, command watchers react when a command is executed.
    /// Where Listeners are for playing an action in the scene, watchers are designed to react on the fly and inject additional behaviours into the command chains.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICommandWatcher<in T> : ICommandWatcher
    {
        bool Accepts(T command) => true;

        void Trigger(T command);
    }
}