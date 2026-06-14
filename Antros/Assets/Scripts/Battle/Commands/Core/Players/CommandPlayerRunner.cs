using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.Players;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    public readonly struct CommandPlayerRunner
    {
        public readonly IGameCommand command;

        public CommandPlayerRunner(IGameCommand command)
        {
            this.command = command;
        }

        public async Awaitable Run(CommandContext context)
        {
            if (!context.TryGetCommandPlayerGroup(command, out ICommandPlayerGroup group))
                return;
            
            await group.Run(context);
        }

    }
}