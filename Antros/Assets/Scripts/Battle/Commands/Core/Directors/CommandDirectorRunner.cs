using UnityEngine;

namespace ATCG.Battle.Commands.Directors
{
    public readonly struct CommandDirectorRunner
    {
        public readonly ICommand command;

        public CommandDirectorRunner(ICommand command)
        {
            this.command = command;
        }

        public async Awaitable Run(CommandContext context)
        {
            if (!context.TryGetGroup(command, out ICommandDirectorGroup group))
                return;

            await group.Run(context);
        }

    }
}