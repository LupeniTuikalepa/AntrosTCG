using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Players
{

    public interface ICommandListenerGroup : IDisposable
    {
        /// <summary>
        /// Start command player execution with the given context
        /// </summary>
        /// <param name="context">Execution context for the command</param>
        Awaitable Run(CommandContext context);
    }

    /// <summary>
    /// Temporary grouping of player that will react to a specific command type.
    /// Used as a cache way to group command player by the types of command they listen to.
    ///
    /// This is used exclusively during a command execution and is not meant to be saved
    /// as new command players could be added later on.
    /// </summary>
    /// <typeparam name="T">Commands to listen to </typeparam>
    public sealed class CommandListenerGroup<T> : ICommandListenerGroup where T : ICommand
    {
        public readonly T command;
        public readonly List<ICommandListener<T>> players;

        public CommandListenerGroup(T command)
        {
            this.command = command;
            players = ListPool<ICommandListener<T>>.Get();
        }

        public void Add(ICommandListener<T> listener)
        {
            players.Add(listener);
        }

        public void Dispose()
        {
            ListPool<ICommandListener<T>>.Release(players);
        }

        public async Awaitable Run(CommandContext context)
        {
            using CommandListenerState state = new(players, 5);

            foreach (ICommandListener<T> player in players)
                player.Play(state, context, command).ListenForExceptions();

            foreach (ICommandListener<T> player in players)
                player.OnBegin(state, context, command);

            await state.WindUp;

            foreach (ICommandListener<T> player in players)
                player.OnHit(state, context, command);

            foreach (ICommand embed in command.GetEmbeds(context))
            {
                CommandListenerRunner runner = new CommandListenerRunner(embed);
                await runner.Run(context);
            }

            await state.FollowThrough;

            foreach (ICommandListener<T> player in players)
                player.OnEnd(state, context, command);
        }


    }
}