using System;

namespace ATCG.Battle.Commands
{
    /// <summary>
    /// Neutral observation surface for the command pipeline, living in the runtime
    /// assembly. It holds plain static delegates typed only in runtime types
    /// (BattleID, ICommand): the runtime INVOKES them, an editor assembly ASSIGNS
    /// them. The dependency therefore runs editor -> runtime only; the runtime never
    /// references the editor, not even through an event subscription.
    ///
    /// No UNITY_EDITOR guard is needed. In a build nobody assigns the delegates, so
    /// each call is a null-check that does nothing. This is ordinary runtime code.
    ///
    /// Two axes feed the timeline:
    ///   - Commands captured at APPLY (CommandContext.Register): every command, root
    ///     and embed, passes through it once, in execution order, Parent/Embeds wired.
    ///   - Groups captured via BeginGroup/EndGroup, since the CommandGroup queue is
    ///     consumed and never retained elsewhere.
    /// </summary>
    public static class CommandTrace
    {
        /// <summary>(groupID, parentGroupID, label) -- a group was opened.</summary>
        public static Action<BattleID, BattleID, string> GroupBegan;

        /// <summary>(groupID) -- a group was closed.</summary>
        public static Action<BattleID> GroupEnded;

        /// <summary>(groupID, rootCommandID) -- a new tree (a .GetSteps()) started; its root follows.</summary>
        public static Action<BattleID, BattleID> TreeBegan;

        /// <summary>(groupID, command) -- a command was registered (applied).</summary>
        public static Action<BattleID, ICommand> CommandRegistered;

        /// <summary>A fresh top-level run begins; assigned listener may reset.</summary>
        public static Action Cleared;

        public static void ReportGroupBegan(BattleID groupID, BattleID parentGroupID, string label)
            => GroupBegan?.Invoke(groupID, parentGroupID, label);

        public static void ReportGroupEnded(BattleID groupID)
            => GroupEnded?.Invoke(groupID);

        public static void ReportTreeBegan(BattleID groupID, BattleID rootCommandID)
            => TreeBegan?.Invoke(groupID, rootCommandID);

        public static void ReportCommandRegistered(BattleID groupID, ICommand command)
            => CommandRegistered?.Invoke(groupID, command);

        public static void ReportCleared()
            => Cleared?.Invoke();
    }
}