using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.Players;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
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