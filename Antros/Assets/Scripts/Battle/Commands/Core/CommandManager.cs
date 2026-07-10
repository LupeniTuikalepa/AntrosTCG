using System.Collections.Generic;
using ATCG.Battle.Commands.Exceptions;
using ATCG.Battle.Commands.Groups;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.Commands.Watchers;
using ATCG.Battle.GameModes;
using Helteix.Singletons.MonoSingletons.Attributes;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Battle.Commands
{
    [DontDestroyOnLoad]
    public static class CommandManager
    {
        public static IReadOnlyCollection<ICommandListener> Listeners => CommandsListeners;

        private static readonly HashSet<ICommandListener> CommandsListeners = new();
        private static readonly HashSet<ICommandWatcher> CommandsWatcher = new();

        private static readonly Stack<CommandGroup> GroupsQueue = new Stack<CommandGroup>();

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            CommandsListeners.Clear();
            GroupsQueue.Clear();
        }

        public static void Run<T>(this T command, BattlePhase battlePhase) where T : ICommand
        {
            RunAsync(command, battlePhase).ListenForExceptions();
        }

        public static CommandGroupHandle BeginGroup(string label)
        {
            CommandGroup group = GroupsQueue.TryPeek(out var parent) ? new CommandGroup(label, parent) : new CommandGroup(label);
            GroupsQueue.Push(group);

            CommandTrace.ReportGroupBegan(group.GroupID, group.ParentGroupID, label);

            return new CommandGroupHandle(group.GroupID);
        }

        public static void EndGroup()
        {
            if(GroupsQueue.TryPeek(out var group))
                EndGroup(group.GroupID);
        }
        public static void EndGroup(BattleID id)
        {
            if (GroupsQueue.TryPeek(out var group) && group.GroupID == id)
            {
                GroupsQueue.Pop();
                CommandTrace.ReportGroupEnded(group.GroupID);
            }
        }

        public static async Awaitable RunAsync<T>(this T command, BattlePhase battlePhase) where T : ICommand
        {
            //If it's the first command called and no group was setup first

            bool useAutoGroup = GroupsQueue.Count == 0;
            if (useAutoGroup)
                BeginGroup($"Auto_{command.GetType().Name}");

            CommandGroup group = GroupsQueue.Peek();
            CommandTree tree = new CommandTree(command);
            group.AddTree(tree);

            CommandTrace.ReportTreeBegan(group.GroupID, command.ID);

            using CommandContext context = new(battlePhase, tree, group.GroupID);
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
            CommandsListeners.Add(listener);
        }

        public static void UnregisterListener(this ICommandListener listener)
        {
            CommandsListeners.Remove(listener);
        }


        public static void RegisterWatcher(this ICommandWatcher watcher)
        {
            CommandsWatcher.Add(watcher);
        }

        public static void UnregisterWatcher(this ICommandWatcher watcher)
        {
            CommandsWatcher.Remove(watcher);
        }

        public static void TriggerWatchers<T>(T command)
        {
            foreach (var watcher in CommandsWatcher)
            {
                if (watcher is not ICommandWatcher<T> w)
                    continue;

                if (w.Accepts(command))
                    w.Trigger(command);
            }
        }
    }
}