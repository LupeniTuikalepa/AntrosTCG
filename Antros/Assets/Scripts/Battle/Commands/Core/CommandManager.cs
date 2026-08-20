using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Commands.Directors;
using ATCG.Battle.Commands.Exceptions;
using ATCG.Battle.Commands.Groups;
using ATCG.Battle.Commands.Listeners;
using ATCG.Battle.GameModes;
using Helteix.Singletons.MonoSingletons.Attributes;
using Helteix.Tools;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace ATCG.Battle.Commands
{
    [DontDestroyOnLoad]
    [AutoStaticsCleanup]
    public static partial class CommandManager
    {
        public static IReadOnlyCollection<ICommandDirector> Listeners => CommandsListeners;

        private static readonly HashSet<ICommandDirector> CommandsListeners = new();
        private static readonly HashSet<ICommandListener> CommandsWatcher = new();

        private static readonly Stack<CommandGroup> GroupsQueue = new Stack<CommandGroup>();

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            CommandsListeners.Clear();
            GroupsQueue.Clear();
            CommandsWatcher.Clear();
        }

        public static void Run<T>(this T command, BattlePhase battlePhase) where T : ICommand
        {
            RunAsync(command, battlePhase).ListenForExceptions();
        }

        public static void Schedule<T>(this T command, BattlePhase battlePhase, CancellationToken token = default) where T : ICommand
        {
            ScheduleAsync(command, battlePhase, token).ListenForExceptions();
        }

        public static async Awaitable ScheduleAsync<T>(this T command, BattlePhase battlePhase, CancellationToken token = default) where T : ICommand
        {
            await Awaitable.EndOfFrameAsync(token);
            await RunAsync(command, battlePhase);
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
            context.Register(context, command);

            try
            {
                command.Process(in context);
            }
            catch (BreakCommandException breakCommandException)
            {
                Debug.Log($"Game Command was canceled because of : {breakCommandException.Cause}");
            }

            CommandDirectorRunner runner = new CommandDirectorRunner(command);
            await runner.Run(context);

            if(useAutoGroup)
                EndGroup();
        }

        public static void Register(this ICommandDirector director)
        {
            CommandsListeners.Add(director);
        }

        public static void Unregister(this ICommandDirector director)
        {
            CommandsListeners.Remove(director);
        }


        public static void Register(this ICommandListener listener)
        {
            CommandsWatcher.Add(listener);
        }

        public static void Unregister(this ICommandListener listener)
        {
            CommandsWatcher.Remove(listener);
        }

        public static void TriggerWatchers<T>(CommandContext context,T command)
        {
            foreach (var watcher in CommandsWatcher)
            {
                if (watcher is not ICommandListener<T> w)
                    continue;
                
                if (w.Accepts(context, command))
                    w.Trigger(context, command);
            }
        }
    }
}