using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Exceptions;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.GameModes;
using Helteix.Singletons.MonoSingletons;
using Helteix.Singletons.MonoSingletons.Attributes;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Commands.Core
{
    [DontDestroyOnLoad]
    public static class GameCommandManager
    {
        private static readonly List<object> CommandsPlayers = new List<object>();

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            CommandsPlayers.Clear();
        }
        
        public static void Run<T>(this T gameCommand, BattlePhase battlePhase) where T: IGameCommand
        {
            RunAsync(gameCommand, battlePhase).FireAndForget();
        }

        public static async Awaitable RunAsync<T>(this T gameCommand, BattlePhase battlePhase) where T: IGameCommand
        {
            using CommandContext context = new(battlePhase, CommandsPlayers);

            context.Register(gameCommand);

            try
            {
                gameCommand.Process(in context);
            }
            catch (BreakCommandException breakCommandException)
            {
                Debug.Log($"Game Command was canceled because of : {breakCommandException.Cause}");
            }

            CommandPlayerRunner runner = new CommandPlayerRunner(gameCommand);
            await runner.Run(context);
        }
        public static void RegisterPlayer<T>(this ICommandPlayer<T> player) where T : IGameCommand
        {
            CommandsPlayers.Add(player);
        }

        public static void UnregisterPlayer<T>(this ICommandPlayer<T> player) where T : IGameCommand
        {
            CommandsPlayers.Remove(player);
        }
    }
}