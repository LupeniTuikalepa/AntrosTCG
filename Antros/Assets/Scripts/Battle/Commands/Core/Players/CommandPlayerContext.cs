using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Players;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    public class CommandPlayerContext<T> : ICommandPlayerContext where T: IGameCommand
    {
        IGameCommand ICommandPlayerContext.Command => command;

        private T command;

        private List<ICommandPlayer<T>> players;

        public CommandPlayerContext(T command)
        {
            this.command = command;
            players = ListPool<ICommandPlayer<T>>.Get();
        }

        public void Add(ICommandPlayer<T> player)
        {
            players.Add(player);
        }

        async Awaitable ICommandPlayerContext.Initiate(GameCommandContext context)
        {
            using (ListPool<Awaitable>.Get(out var tasks))
            {
                foreach (ICommandPlayer<T> player in players)
                    tasks.Add(PlayCommandPlayer(player));

                await tasks.WhenAll();
            }
        }

        private async Awaitable PlayCommandPlayer(ICommandPlayer<T> commandPlayer)
        {
            try
            {
                await commandPlayer.Play(command);
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