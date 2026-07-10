using System.Collections.Generic;
using ATCG.Battle;
using ATCG.Battle.Commands;
using ATCG.Battle.Entities.Components;

namespace ATCG.Editor.Editor.Commands
{
    /// <summary>
    /// One captured command. The live reference is kept so the inspector can read
    /// field values lazily; structure is resolved at read time (BuildLinks) rather
    /// than at registration, because Embed() calls Register BEFORE SetParent, so a
    /// command's Parent is still None at the moment its trace event fires.
    /// </summary>
    public sealed class TracedCommand
    {
        public BattleID ID;
        public BattleID GroupID;
        public string TypeName;
        public ICommand Command;

        /// <summary>True if this command is the root of a tree (born from a .GetSteps()).</summary>
        public bool IsTreeRoot;

        /// <summary>Capture order; drives the vertical stacking of chained trees.</summary>
        public int Order;

        public readonly List<TracedCommand> Children = new();
    }

    public sealed class TracedGroup
    {
        public BattleID GroupID;
        public BattleID ParentGroupID;
        public string Label;
        public bool Closed;
        public int Order;

        public readonly List<TracedGroup> ChildGroups = new();

        /// <summary>Tree roots of this group, in arrival order (the vertical axis).</summary>
        public readonly List<TracedCommand> Roots = new();
    }

    /// <summary>
    /// Accumulates the trace stream.
    ///
    /// Axes (per the latest spec):
    ///   - Vertical   = chained trees: each .GetSteps() in a group is one root, stacked
    ///                  top to bottom in arrival order.
    ///   - Horizontal = embed depth: a command's embeds extend to the right.
    ///
    /// Roots are known authoritatively from TreeBegan. Embeds are linked to their
    /// parent by the live Parent field, resolved in BuildLinks() at read time once
    /// SetParent has run.
    /// </summary>
    public sealed class CommandTraceModel
    {
        private readonly Dictionary<BattleID, TracedGroup> groups = new();
        private readonly Dictionary<BattleID, TracedCommand> commands = new();
        private readonly List<TracedGroup> rootGroups = new();
        private readonly HashSet<BattleID> declaredRoots = new();

        private int orderCounter;

        public IReadOnlyList<TracedGroup> RootGroups => rootGroups;
        public int Count => commands.Count;

        public void Clear()
        {
            groups.Clear();
            commands.Clear();
            rootGroups.Clear();
            declaredRoots.Clear();
            orderCounter = 0;
        }

        public void OnGroupBegan(BattleID groupID, BattleID parentGroupID, string label)
        {
            if (groups.ContainsKey(groupID))
                return;

            TracedGroup group = new()
            {
                GroupID = groupID,
                ParentGroupID = parentGroupID,
                Label = label,
                Order = orderCounter++,
            };
            groups[groupID] = group;

            if (parentGroupID != BattleID.None && groups.TryGetValue(parentGroupID, out TracedGroup parent))
                parent.ChildGroups.Add(group);
            else
                rootGroups.Add(group);
        }

        public void OnGroupEnded(BattleID groupID)
        {
            if (groups.TryGetValue(groupID, out TracedGroup group))
                group.Closed = true;
        }

        /// <summary>The next command registered with this id is a tree root, not an embed.</summary>
        public void OnTreeBegan(BattleID groupID, BattleID rootCommandID)
        {
            declaredRoots.Add(rootCommandID);
        }

        public void OnCommandRegistered(BattleID groupID, ICommand command)
        {
            if (command == null || commands.ContainsKey(command.ID))
                return;

            if (!groups.TryGetValue(groupID, out TracedGroup group))
            {
                OnGroupBegan(groupID, BattleID.None, "(untracked group)");
                group = groups[groupID];
            }

            TracedCommand traced = new()
            {
                ID = command.ID,
                GroupID = groupID,
                TypeName = command.GetType().Name,
                Command = command,
                IsTreeRoot = declaredRoots.Contains(command.ID),
                Order = orderCounter++,
            };
            commands[command.ID] = traced;

            if (traced.IsTreeRoot)
                group.Roots.Add(traced);
        }

        /// <summary>
        /// Resolve embed parent/child links from the now-valid Parent fields. Call
        /// once before rendering. Idempotent: clears prior links first.
        /// </summary>
        public void BuildLinks()
        {
            foreach (TracedCommand c in commands.Values)
                c.Children.Clear();

            foreach (TracedCommand c in commands.Values)
            {
                if (c.IsTreeRoot || c.Command == null)
                    continue;

                BattleID parentID = c.Command.Parent;
                if (parentID != BattleID.None && commands.TryGetValue(parentID, out TracedCommand parent))
                    parent.Children.Add(c);
                else
                {
                    // Parent not captured (shouldn't happen): surface it as a root of
                    // its group so it isn't silently dropped.
                    if (groups.TryGetValue(c.GroupID, out TracedGroup g) && !g.Roots.Contains(c))
                        g.Roots.Add(c);
                }
            }

            foreach (TracedCommand c in commands.Values)
                c.Children.Sort((a, b) => a.Order.CompareTo(b.Order));
            foreach (TracedGroup g in groups.Values)
                g.Roots.Sort((a, b) => a.Order.CompareTo(b.Order));
        }
    }
}