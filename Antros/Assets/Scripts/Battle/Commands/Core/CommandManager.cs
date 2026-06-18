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
    public static class CommandManager
    {
        private static readonly List<ICommandListener> CommandsPlayers = new List<ICommandListener>();

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            CommandsPlayers.Clear();
        }

        public static void Run<T>(this T gameCommand, BattlePhase battlePhase) where T: ICommand
        {
            RunAsync(gameCommand, battlePhase).ListenForExceptions();
        }

        public static async Awaitable RunAsync<T>(this T gameCommand, BattlePhase battlePhase) where T: ICommand
        {
            using CommandCollection collection = new CommandCollection(gameCommand);
            using CommandContext context = new(battlePhase, CommandsPlayers, collection);

            context.Register(gameCommand);

            try
            {
                gameCommand.Process(in context);
            }
            catch (BreakCommandException breakCommandException)
            {
                Debug.Log($"Game Command was canceled because of : {breakCommandException.Cause}");
            }

            CommandListenerRunner runner = new CommandListenerRunner(gameCommand);
            await runner.Run(context);
        }
        public static void RegisterListener(this ICommandListener listener)
        {
            CommandsPlayers.Add(listener);
        }

        public static void UnregisterListener(this ICommandListener listener)
        {
            CommandsPlayers.Remove(listener);
        }
    }
}