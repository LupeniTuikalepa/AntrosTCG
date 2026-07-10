using UnityEngine;

namespace ATCG.Battle.Commands.Listeners
{
    public readonly struct CommandListenerRunner
    {
        public readonly ICommand command;

        public CommandListenerRunner(ICommand command)
        {
            this.command = command;
        }

        public async Awaitable Run(CommandContext context)
        {
            if (!context.TryGetGroup(command, out ICommandListenerGroup group))
                return;

            await group.Run(context);
        }

    }
}