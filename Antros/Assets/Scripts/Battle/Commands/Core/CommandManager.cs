using System;
using System.Collections.Generic;
using ATCG.Battle.Commands.Core.Exceptions;
using ATCG.Battle.Commands.Core.Players;
using ATCG.Battle.Entities.Components;
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
        private static readonly Queue<BattleID> groupsQueue = new Queue<BattleID>();

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            CommandsPlayers.Clear();
            groupsQueue.Clear();
        }

        public static void Run(this ICommand command, BattlePhase battlePhase)
        {
            RunAsync(command, battlePhase).ListenForExceptions();
        }

        public static BattleID BeginGroup()
        {
            BattleID battleID = BattleID.CreateNew();
            groupsQueue.Enqueue(battleID);
            return battleID;
        }

        public static BattleID EndGroup() => groupsQueue.Dequeue();

        public static async Awaitable RunAsync(this ICommand command, BattlePhase battlePhase)
        {

            bool isInGroup = false;

            if (groupsQueue.TryDequeue(out BattleID groupID))
            {
                isInGroup = true;
            }
            else
            {
                groupID = BeginGroup();
                isInGroup = false;
            }

            using CommandCollection collection = new CommandCollection(command);
            using CommandContext context = new(battlePhase, CommandsPlayers, collection, groupID);
            context.Register(command);

            try
            {
                command.Process(in context);
            }
            catch (BreakCommandException breakCommandException)
            {
                Debug.Log($"Game Command was canceled because of : {breakCommandException.Cause}");
            }

            CommandListenerRunner runner = new CommandListenerRunner(command);
            await runner.Run(context);

            if (!isInGroup)
                EndGroup();
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