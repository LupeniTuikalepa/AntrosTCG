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
        private static readonly Queue<CommandGroup> groupsQueue = new Queue<CommandGroup>();

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            CommandsPlayers.Clear();
            groupsQueue.Clear();
        }

        public static void Run<T>(this T command, BattlePhase battlePhase) where T : ICommand
        {
            RunAsync(command, battlePhase).ListenForExceptions();
        }

        public static BattleID BeginGroup(string label)
        {
            CommandGroup group = groupsQueue.TryPeek(out var parent) ? new CommandGroup(label, parent) : new CommandGroup(label);
            groupsQueue.Enqueue(group);

            Trace.CommandTrace.ReportGroupBegan(group.GroupID, group.ParentGroupID, label);

            return group.GroupID;
        }

        public static BattleID EndGroup()
        {
            CommandGroup group = groupsQueue.Dequeue();
            Trace.CommandTrace.ReportGroupEnded(group.GroupID);
            return group.GroupID;
        }

        public static async Awaitable RunAsync<T>(this T command, BattlePhase battlePhase) where T : ICommand
        {
            //If it's the first command called and no group was setup first

            bool useAutoGroup = groupsQueue.Count == 0;
            if (useAutoGroup)
                BeginGroup($"Auto_{command.GetType().Name}");

            CommandGroup group = groupsQueue.Peek();
            CommandTree tree = new CommandTree(command);
            group.AddTree(tree);

            Trace.CommandTrace.ReportTreeBegan(group.GroupID, command.ID);

            using CommandContext context = new(battlePhase, CommandsPlayers, tree, group.GroupID);
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

            if(useAutoGroup)
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