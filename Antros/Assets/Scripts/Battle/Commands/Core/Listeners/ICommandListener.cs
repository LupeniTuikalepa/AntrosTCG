namespace ATCG.Battle.Commands.Listeners
{
    public interface ICommandListener { }

    /// <summary>
    /// Like listeners, command watchers react when a command is executed.
    /// Where Listeners are for playing an action in the scene, watchers are designed to react on the fly and inject additional behaviours into the command chains.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICommandListener<in T> : ICommandListener
    {
        bool Accepts(CommandContext context, T command) => true;

        void Trigger(CommandContext context, T command);
    }
}