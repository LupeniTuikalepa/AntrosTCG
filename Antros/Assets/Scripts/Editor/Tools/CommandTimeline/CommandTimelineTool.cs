using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.Trace;
using ATCG.Battle.Entities.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ATCG.Editor;

namespace ATCG.Editor.Editor.Commands
{
    /// <summary>
    /// Timeline view of the command pipeline.
    ///
    ///   Vertical    = chained trees: each .GetSteps() in a group is one row, stacked
    ///                 top to bottom in arrival order.
    ///   Horizontal  = embed depth: a command's embeds extend to the right.
    ///
    /// The middle bar is a draggable splitter (TwoPaneSplitView). All styling lives
    /// in CommandTimeline.uss. Passive observer of the runtime CommandTrace.
    /// </summary>
    public sealed class CommandTimelineTool : IEditorTool, System.IDisposable
    {
        private const string UssGuidHint = "CommandTimeline.uss";

        private static readonly Color ArrowColor = new(0.45f, 0.55f, 0.72f);

        private readonly CommandTraceModel model = new();

        private bool dirty;

        private VisualElement root;
        private ScrollView timelineScroll;
        private VisualElement inspectorBody;
        private Label statusLabel;
        private Button clearFocusButton;
        private TracedCommand selected;

        // Cross-tool: when set, only commands whose IEntityCommand.Target matches are
        // highlighted. -1 means "no entity focus" (show everything normally).
        private int focusedEntityId = -1;

        // per-rebuild scratch for wiring arrows
        private readonly Dictionary<BattleID, VisualElement> cardByID = new();
        private readonly List<(BattleID parent, BattleID child, ConnectorLayer layer)> pendingLinks = new();
        private readonly List<ConnectorLayer> connectorLayers = new();

        public string DisplayName => "Command Timeline";
        public string Icon => "\u21F6"; // three rightwards arrows / pipeline
        public int Order => 20;

        public CommandTimelineTool()
        {
            // Subscribe to the trace stream for the whole lifetime of the tool (which
            // lives as long as the hub window is open), NOT just while this tool is the
            // visible tab. Otherwise commands fired while another tool is shown are lost.
            // The handlers only mutate the model + set a dirty flag; they never touch UI.
            CommandTrace.GroupBegan += OnGroupBegan;
            CommandTrace.GroupEnded += OnGroupEnded;
            CommandTrace.TreeBegan += OnTreeBegan;
            CommandTrace.CommandRegistered += OnCommandRegistered;
            CommandTrace.Cleared += OnCleared;
        }

        public VisualElement BuildUI()
        {
            BuildChrome();
            Rebuild();
            return root;
        }

        public void OnActivated()
        {
            // Only the *visual* refresh is tied to activation; the trace subscription
            // above keeps running in the background regardless.
            EditorApplication.update += PollDirty;
            EditorToolBus.Subscribe<FocusEntityRequest>(OnFocusEntity);
            Rebuild();
        }

        public void OnDeactivated()
        {
            EditorApplication.update -= PollDirty;
            EditorToolBus.Unsubscribe<FocusEntityRequest>(OnFocusEntity);
        }

        private void OnFocusEntity(FocusEntityRequest req)
        {
            focusedEntityId = req.EntityId;
            Rebuild();
        }

        public void Dispose()
        {
            CommandTrace.GroupBegan -= OnGroupBegan;
            CommandTrace.GroupEnded -= OnGroupEnded;
            CommandTrace.TreeBegan -= OnTreeBegan;
            CommandTrace.CommandRegistered -= OnCommandRegistered;
            CommandTrace.Cleared -= OnCleared;
        }

        // ---- trace stream ----

        private void OnGroupBegan(BattleID id, BattleID parent, string label)
        {
            model.OnGroupBegan(id, parent, label);
            dirty = true;
        }

        private void OnGroupEnded(BattleID id)
        {
            model.OnGroupEnded(id);
            dirty = true;
        }

        private void OnTreeBegan(BattleID groupID, BattleID rootCommandID)
        {
            model.OnTreeBegan(groupID, rootCommandID);
            dirty = true;
        }

        private void OnCommandRegistered(BattleID groupID, ICommand command)
        {
            model.OnCommandRegistered(groupID, command);
            dirty = true;
        }

        private void OnCleared()
        {
            model.Clear();
            selected = null;
            dirty = true;
        }

        private void PollDirty()
        {
            if (!dirty)
                return;
            dirty = false;
            Rebuild();
        }

        // ---- chrome ----

        private void BuildChrome()
        {
            root = new VisualElement();
            root.style.flexGrow = 1;
            root.style.flexDirection = FlexDirection.Column;

            EditorStyles.Load(root, "EditorTheme.uss");
            EditorStyles.Load(root, UssGuidHint);

            VisualElement toolbar = new();
            toolbar.AddToClassList("ctl-toolbar");

            Button clearButton = new(ClearAll) { text = "Clear" };
            toolbar.Add(clearButton);

            Button clearFocus = new(() => { focusedEntityId = -1; Rebuild(); }) { text = "Clear entity focus" };
            clearFocus.AddToClassList("ctl-clear-focus");
            toolbar.Add(clearFocus);

            statusLabel = new Label();
            statusLabel.AddToClassList("ctl-status");
            toolbar.Add(statusLabel);
            root.Add(toolbar);

            // The focus-clear button only matters when a focus is active.
            clearFocus.style.display = focusedEntityId >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            this.clearFocusButton = clearFocus;

            // draggable splitter: timeline on the left, inspector on the right
            TwoPaneSplitView split = new(1, 260, TwoPaneSplitViewOrientation.Horizontal);
            split.style.flexGrow = 1;
            root.Add(split);

            timelineScroll = new ScrollView(ScrollViewMode.Vertical);
            timelineScroll.AddToClassList("ctl-canvas");
            timelineScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            timelineScroll.verticalScrollerVisibility = ScrollerVisibility.Auto;
            split.Add(timelineScroll);

            VisualElement inspector = new();
            inspector.AddToClassList("ctl-inspector");
            split.Add(inspector);

            Label inspectorTitle = new("Details");
            inspectorTitle.AddToClassList("ctl-inspector__title");
            inspector.Add(inspectorTitle);

            ScrollView inspectorScroll = new(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
            inspector.Add(inspectorScroll);

            inspectorBody = new VisualElement();
            inspectorBody.AddToClassList("ctl-inspector__body");
            inspectorScroll.Add(inspectorBody);
        }


        private void ClearAll()
        {
            model.Clear();
            selected = null;
            Rebuild();
        }

        private void Rebuild()
        {
            if (timelineScroll == null)
                return;

            model.BuildLinks();

            cardByID.Clear();
            pendingLinks.Clear();
            connectorLayers.Clear();
            displayIndexByCmd.Clear();
            timelineScroll.Clear();

            VisualElement page = new();
            page.AddToClassList("ctl-page");

            foreach (TracedGroup group in model.RootGroups)
                page.Add(BuildGroup(group, 0));

            timelineScroll.Add(page);

            string focusNote = focusedEntityId >= 0 ? $"  \u00b7  focus: entity {focusedEntityId}" : "";
            statusLabel.text = model.Count == 0
                ? "Waiting for commands. GetSteps an action in play mode."
                : $"{model.Count} commands  \u00b7  {CountGroups(model.RootGroups)} groups{focusNote}";

            if (clearFocusButton != null)
                clearFocusButton.style.display = focusedEntityId >= 0 ? DisplayStyle.Flex : DisplayStyle.None;

            // arrows need final geometry; resolve links once layout settles
            page.RegisterCallback<GeometryChangedEvent>(OnPageGeometry);

            RebuildInspector();
        }

        private void OnPageGeometry(GeometryChangedEvent evt)
        {
            foreach (ConnectorLayer layer in connectorLayers)
                layer.Clear();

            foreach ((BattleID parentID, BattleID childID, ConnectorLayer layer) in pendingLinks)
            {
                if (layer == null)
                    continue;
                if (!cardByID.TryGetValue(parentID, out VisualElement parentCard))
                    continue;
                if (!cardByID.TryGetValue(childID, out VisualElement childCard))
                    continue;

                layer.AddLink(parentCard, childCard);
            }

            foreach (ConnectorLayer layer in connectorLayers)
                layer.Refresh();
        }

        private static int CountGroups(IReadOnlyList<TracedGroup> list)
        {
            int n = 0;
            foreach (TracedGroup g in list)
            {
                n++;
                n += CountGroups(g.ChildGroups);
            }
            return n;
        }

        // ---- group box ----

        private VisualElement BuildGroup(TracedGroup group, int depth)
        {
            VisualElement box = new();
            box.AddToClassList("ctl-group");
            if (depth > 0)
                box.AddToClassList("ctl-group--nested");

            VisualElement header = new();
            header.AddToClassList("ctl-group__header");

            VisualElement rail = new();
            rail.AddToClassList("ctl-group__rail");
            if (!group.Closed)
                rail.AddToClassList("ctl-group__rail--open");
            header.Add(rail);

            Label title = new(group.Label);
            title.AddToClassList("ctl-group__title");
            header.Add(title);

            if (!group.Closed)
            {
                Label openBadge = new("open");
                openBadge.AddToClassList("ctl-pill");
                header.Add(openBadge);
            }
            box.Add(header);

            // body: a horizontal scroll view, so each group scrolls independently
            // and keeps its header in view. The connector overlay and the rows both
            // live in the scroll's content container, so they translate together and
            // the arrows stay glued to the cards while scrolling.
            ScrollView bodyScroll = new(ScrollViewMode.Horizontal);
            bodyScroll.AddToClassList("ctl-group__body");
            bodyScroll.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            bodyScroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;

            VisualElement content = bodyScroll.contentContainer;
            content.style.flexDirection = FlexDirection.Column;
            content.style.position = Position.Relative;

            ConnectorLayer layer = new() { LineColor = ArrowColor };
            connectorLayers.Add(layer);
            content.Add(layer);

            foreach (TracedCommand root in group.Roots)
                content.Add(BuildRow(root, layer));

            bodyScroll.horizontalScroller.valueChanged += _ => layer.Refresh();
            box.Add(bodyScroll);

            if (group.ChildGroups.Count > 0)
            {
                VisualElement nested = new();
                nested.AddToClassList("ctl-group__nested");
                foreach (TracedGroup child in group.ChildGroups)
                    nested.Add(BuildGroup(child, depth + 1));
                box.Add(nested);
            }

            return box;
        }

        // ---- a row: a tree root with its embeds extending to the right ----

        private readonly Dictionary<BattleID, int> displayIndexByCmd = new();

        private VisualElement BuildRow(TracedCommand root, ConnectorLayer layer)
        {
            VisualElement row = new();
            row.AddToClassList("ctl-row");
            // Per-root numbering: each root restarts the visible index at 0 so the order
            // within a tree is easy to read. Stored Order stays global (used for sorting).
            int counter = 0;
            AssignDisplayIndices(root, ref counter);
            BuildSubtree(row, root, isRoot: true, layer);
            return row;
        }

        // Depth-first in the same order cards are laid out, so the visible numbers run
        // 0,1,2,... down each root's tree.
        private void AssignDisplayIndices(TracedCommand cmd, ref int counter)
        {
            displayIndexByCmd[cmd.ID] = counter++;
            foreach (TracedCommand child in cmd.Children)
                AssignDisplayIndices(child, ref counter);
        }

        /// <summary>
        /// Lays a command card, then its embeds as a vertical column of siblings to
        /// its right (depth extends rightward). Records a link per parent->child so
        /// the overlay can draw an arrow between them once geometry resolves.
        /// </summary>
        private void BuildSubtree(VisualElement horizontalBand, TracedCommand cmd, bool isRoot, ConnectorLayer layer)
        {
            horizontalBand.Add(BuildCard(cmd, isRoot));

            if (cmd.Children.Count == 0)
                return;

            VisualElement embedColumn = new();
            embedColumn.AddToClassList("ctl-embed-group");

            foreach (TracedCommand child in cmd.Children)
            {
                pendingLinks.Add((cmd.ID, child.ID, layer));

                VisualElement childBand = new();
                childBand.AddToClassList("ctl-row");
                BuildSubtree(childBand, child, isRoot: false, layer);
                embedColumn.Add(childBand);
            }

            horizontalBand.Add(embedColumn);
        }

        private VisualElement BuildCard(TracedCommand cmd, bool isRoot)
        {
            VisualElement card = new();
            card.AddToClassList("ctl-card");
            if (isRoot)
                card.AddToClassList("ctl-card--root");
            if (cmd == selected)
                card.AddToClassList("ctl-card--selected");

            // Cross-tool entity focus: when an entity is focused from another tool,
            // commands whose IEntityCommand.Target is that entity are highlighted, the
            // rest dimmed. IEntityCommand.Target is typed, so no reflection needed.
            if (focusedEntityId >= 0)
            {
                bool hit = cmd.Command is IEntityCommand ec && ec.Target.id == focusedEntityId;
                card.AddToClassList(hit ? "ctl-card--focus-hit" : "ctl-card--focus-dim");
            }

            cardByID[cmd.ID] = card;

            VisualElement titleRow = new();
            titleRow.AddToClassList("ctl-card__title-row");

            VisualElement dot = new();
            dot.AddToClassList("ctl-card__dot");
            if (isRoot)
                dot.AddToClassList("ctl-card__dot--root");
            titleRow.Add(dot);

            Label title = new(cmd.TypeName);
            title.AddToClassList("ctl-card__title");
            titleRow.Add(title);
            card.Add(titleRow);

            int shown = displayIndexByCmd.TryGetValue(cmd.ID, out int di) ? di : cmd.Order;
            Label meta = new(cmd.Children.Count > 0
                ? $"#{shown}  \u00b7  {cmd.Children.Count} embeds"
                : $"#{shown}");
            meta.AddToClassList("ctl-card__meta");
            card.Add(meta);

            card.RegisterCallback<MouseDownEvent>(_ =>
            {
                selected = cmd;
                Rebuild();
            });

            return card;
        }

        // ---- inspector ----

        private void RebuildInspector()
        {
            inspectorBody.Clear();

            if (selected == null)
            {
                Label hint = new("Select a command card.");
                hint.AddToClassList("ctl-hint");
                inspectorBody.Add(hint);
                return;
            }

            Label typeName = new(selected.TypeName);
            typeName.AddToClassList("ctl-inspector__type");
            inspectorBody.Add(typeName);

            AddRow("ID", FormatBattleID(selected.ID));
            int shownOrder = displayIndexByCmd.TryGetValue(selected.ID, out int di) ? di : selected.Order;
            AddRow("Order", shownOrder.ToString());
            AddRow("Embeds", selected.Children.Count.ToString());
            AddRow("Kind", selected.IsTreeRoot ? "tree root" : "embed");

            if (selected.Command != null)
            {
                SectionHeader("Fields");
                DumpFields(selected.Command, selected.Command.GetType(), 0);
            }
        }

        private const BindingFlags FieldFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private void DumpFields(object target, System.Type type, int depth)
        {
            if (target == null || depth > 4)
                return;

            for (System.Type t = type; t != null && t != typeof(object); t = t.BaseType)
            {
                foreach (FieldInfo field in t.GetFields(FieldFlags))
                {
                    if (!IsShown(field))
                        continue;

                    string name = CleanName(field.Name);

                    object value;
                    try { value = field.GetValue(target); }
                    catch { AddRow(name, "(unreadable)", depth); continue; }

                    if (field.FieldType == typeof(BattleID))
                    {
                        AddRow(name, FormatBattleID(value), depth);
                    }
                    else if (ShouldExpand(field.FieldType, value))
                    {
                        SubHeader(name, depth);
                        DumpFields(value, field.FieldType, depth + 1);
                    }
                    else
                    {
                        AddRow(name, Format(value), depth);
                    }
                }
            }
        }

        private static bool IsShown(FieldInfo field)
        {
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(field.FieldType)
                && field.FieldType != typeof(string))
                return false;

            if (field.IsPublic)
                return true;
            return field.IsDefined(typeof(SerializeField), inherit: true);
        }

        private static bool ShouldExpand(System.Type type, object value)
        {
            if (value == null)
                return false;
            if (!type.IsValueType || type.IsPrimitive || type.IsEnum)
                return false;
            if (type == typeof(decimal) || type == typeof(System.DateTime))
                return false;
            if (type == typeof(BattleID))
                return false;
            return type.Namespace == null || type.Namespace.StartsWith("ATCG");
        }

        /// <summary>
        /// BattleID has no ToString override, so value.ToString() prints the type
        /// name. Read its instance fields by reflection to surface the actual value
        /// regardless of whether it's backed by an int, ulong, Guid, etc.
        /// </summary>
        private static string FormatBattleID(object value)
        {
            if (value == null)
                return "null";

            System.Type type = value.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fields.Length == 0)
                return value.ToString();

            if (fields.Length == 1)
                return fields[0].GetValue(value)?.ToString() ?? "null";

            StringBuilder sb = new();
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(CleanName(fields[i].Name)).Append('=').Append(fields[i].GetValue(value));
            }
            return sb.ToString();
        }

        private static string CleanName(string raw)
        {
            if (raw.Length > 0 && raw[0] == '<')
            {
                int end = raw.IndexOf('>');
                if (end > 1)
                    return raw.Substring(1, end - 1);
            }
            return raw;
        }

        private static string Format(object value)
        {
            if (value == null)
                return "null";
            if (value is float f)
                return f.ToString("0.###");
            if (value is double d)
                return d.ToString("0.###");
            return value.ToString();
        }

        // ---- inspector rows ----

        private void AddRow(string label, string value, int depth = 0)
        {
            VisualElement row = new();
            row.AddToClassList("ctl-field");
            if (depth > 0)
                row.style.marginLeft = depth * 10;

            Label l = new(label);
            l.AddToClassList("ctl-field__label");
            Label v = new(value);
            v.AddToClassList("ctl-field__value");
            row.Add(l);
            row.Add(v);
            inspectorBody.Add(row);
        }

        private void SectionHeader(string text)
        {
            VisualElement sep = new();
            sep.AddToClassList("ctl-section");
            inspectorBody.Add(sep);

            Label h = new(text);
            h.AddToClassList("ctl-section__label");
            inspectorBody.Add(h);
        }

        private void SubHeader(string text, int depth)
        {
            Label h = new(text);
            h.AddToClassList("ctl-subheader");
            if (depth > 0)
                h.style.marginLeft = depth * 10;
            inspectorBody.Add(h);
        }
    }
}
