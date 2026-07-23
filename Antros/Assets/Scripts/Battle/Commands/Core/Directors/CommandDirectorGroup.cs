using System.Collections.Generic;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Directors
{
    /// <summary>
    /// Temporary grouping of player that will react to a specific command type.
    /// Used as a cache way to group command player by the types of command they listen to.
    ///
    /// This is used exclusively during a command execution and is not meant to be saved
    /// as new command players could be added later on.
    /// </summary>
    /// <typeparam name="T">Commands to listen to </typeparam>
    public sealed class CommandDirectorGroup<T> : ICommandDirectorGroup where T : ICommand
    {
        public readonly T command;
        public readonly List<ICommandDirector<T>> players;

        public CommandDirectorGroup(T command)
        {
            this.command = command;
            players = ListPool<ICommandDirector<T>>.Get();
        }

        public void Add(ICommandDirector<T> director)
        {
            players.Add(director);
        }

        public void Dispose()
        {
            ListPool<ICommandDirector<T>>.Release(players);
        }

        public async Awaitable Run(CommandContext context)
        {
            using CommandDirectorState state = new(players, 5);

            foreach (ICommandDirector<T> player in players)
                player.Play(state, context, command).ListenForExceptions();

            foreach (ICommandDirector<T> player in players)
                player.OnBegin(state, context, command);

            await state.WindUp;

            foreach (ICommandDirector<T> player in players)
                player.OnHit(state, context, command);

            foreach (ICommand embed in command.GetEmbeds(context))
            {
                CommandDirectorRunner runner = new CommandDirectorRunner(embed);
                await runner.Run(context);
            }

            await state.FollowThrough;

            foreach (ICommandDirector<T> player in players)
                player.OnEnd(state, context, command);
        }
    }
}