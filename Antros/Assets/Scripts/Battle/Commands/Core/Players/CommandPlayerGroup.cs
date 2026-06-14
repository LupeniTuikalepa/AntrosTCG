using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Players
{

    public interface ICommandPlayerGroup : IDisposable
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
    public sealed class CommandPlayerGroup<T> : ICommandPlayerGroup where T : IGameCommand
    {
        public readonly T command;
        public readonly List<ICommandPlayer<T>> players;

        public CommandPlayerGroup(T command)
        {
            this.command = command;
            players = ListPool<ICommandPlayer<T>>.Get();
        }

        public void Add(ICommandPlayer<T> player)
        {
            players.Add(player);
        }
        
        public void Dispose()
        {
            ListPool<ICommandPlayer<T>>.Release(players);
        }

        public async Awaitable Run(CommandContext context)
        {
            using CommandPlayerState state = new();

            foreach (ICommandPlayer<T> player in players)
                player.Play(state, context, command).FireAndForget();

            foreach (ICommandPlayer<T> player in players)
                player.OnBegin(state, context, command);
            
            await state.WindUp;
            
            foreach (ICommandPlayer<T> player in players)
                player.OnHit(state, context, command);

            foreach (IGameCommand embed in command.Embeds)
            {
                CommandPlayerRunner runner = new CommandPlayerRunner(embed);
                await runner.Run(context);
            }
            
            await state.FollowThrough;
            
            foreach (ICommandPlayer<T> player in players)
                player.OnEnd(state, context, command);
        }


    }
}