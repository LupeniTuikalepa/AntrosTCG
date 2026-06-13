using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Commands.Players;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    /// <summary>
    /// Temporary grouping of player that will react to a specific command type.
    /// Used as a cache way to group command player by the types of command they listen to.
    ///
    /// This is used exclusively during a command execution and is not meant to be saved
    /// as new command players could be added later on.
    /// </summary>
    /// <typeparam name="T">Commands to listen to </typeparam>
    public sealed class CommandPlayerRunner<T>  where T: IGameCommand
    {
        public readonly T command;

        public CommandPlayerRunner(T command)
        {
            this.command = command;
        }
        public async Awaitable Run(CommandContext context)
        {

        }

        private static async Awaitable RunCommand(CommandContext context, IGameCommand command)
        {
            if (!context.TryGetCommandPlayerGroup(command, out ICommandPlayerGroup group))
                return;
            AwaitableCompletionSource windUpSource = new AwaitableCompletionSource();
            AwaitableCompletionSource followThroughSource = new AwaitableCompletionSource();

            var windUp = windUpSource.Awaitable;
            var followThrough = followThroughSource.Awaitable;

            foreach (var player in group.)
            {
                CommandPlayerState playerState = new CommandPlayerState();
                player.Play(context, command);
            }

            foreach (IGameCommand embed in command.Embeds)
            {
                await windUp;

                await RunCommand(context, embed);

                await followThrough;
            }
        }
    }
}