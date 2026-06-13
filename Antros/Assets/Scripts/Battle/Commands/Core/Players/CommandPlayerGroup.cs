using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Core.Players;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Players
{

    public interface ICommandPlayerGroup : IDisposable
    {
        
    }

    /// <summary>
    /// Temporary grouping of player that will react to a specific command type.
    /// Used as a cache way to group command player by the types of command they listen to.
    ///
    /// This is used exclusively during a command execution and is not meant to be saved
    /// as new command players could be added later on.
    /// </summary>
    /// <typeparam name="T">Commands to listen to </typeparam>
    public class CommandPlayerGroup<T> : ICommandPlayerGroup where T : IGameCommand
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
    }
}