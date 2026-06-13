using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Players;
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
    public sealed class CommandPlayerGroup<T> : ICommandPlayerGroup where T: IGameCommand
    {
        IGameCommand ICommandPlayerGroup.Command => command;

        private readonly List<ICommandPlayer<T>> players;
        private readonly T command;

        public CommandPlayerGroup(T command)
        {
            this.command = command;
            players = ListPool<ICommandPlayer<T>>.Get();
        }

        public void Add(ICommandPlayer<T> player)
        {
            players.Add(player);
        }

        async Awaitable ICommandPlayerGroup.Initiate(GameCommandContext context)
        {
            using (ListPool<Awaitable>.Get(out var tasks))
            {
                foreach (ICommandPlayer<T> player in players)
                    tasks.Add(PlayCommandPlayer(context, player));

                await tasks.WhenAll();
            }
        }

        private async Awaitable PlayCommandPlayer(GameCommandContext context, ICommandPlayer<T> commandPlayer)
        {
            try
            {
                await commandPlayer.Play(context, command);
            }
            catch (Exception e)
            {
                await Awaitable.MainThreadAsync();
                Debug.LogException(e);
            }
        }

        void IDisposable.Dispose()
        {
            ListPool<ICommandPlayer<T>>.Release(players);
        }

    }
}