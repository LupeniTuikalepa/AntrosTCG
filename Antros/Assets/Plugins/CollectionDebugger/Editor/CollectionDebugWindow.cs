using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Pool;
using CollectionDebugger.Core;

namespace CollectionDebugger.Editor
{
    public class CollectionDebugWindow : EditorWindow
    {
        private static readonly Color ColorSectionLive = new(0.3f, 0.7f, 0.4f);
        private static readonly Color ColorSectionSnap = new(0.8f, 0.6f, 0.2f);
        private static readonly Color ColorSeparator   = new(0.2f, 0.2f, 0.2f);
        private static readonly Color ColorBadgeBg     = new(0.15f, 0.15f, 0.15f);

        private ListView watchListView;
        private ScrollView rightPanel;
        private readonly List<ICollectionWatch> watchList = new();
        private int lastWatchCount;
        private ICollectionWatch selectedWatch;

        private Foldout liveFoldout;
        private readonly List<Foldout> snapshotFoldouts = new();
        private readonly List<SnapshotWatch> drawnSnapshots = new();

        [MenuItem("Tools/Collection Debugger")]
        public static void Open() => GetWindow<CollectionDebugWindow>("Collection Debugger");

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);

            var splitView = new TwoPaneSplitView(0, 160, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            BuildLeftPanel(splitView);
            BuildRightPanel(splitView);
        }

        private void BuildLeftPanel(TwoPaneSplitView splitView)
        {
            var left = new VisualElement();
            left.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);

            var header = new Label("WATCHES");
            header.style.fontSize = 10;
            header.style.color = new Color(0.5f, 0.5f, 0.5f);
            header.style.paddingLeft = 10;
            header.style.paddingTop = 8;
            header.style.paddingBottom = 6;
            header.style.letterSpacing = 1.5f;
            left.Add(header);

            left.Add(BuildHorizontalSeparator());

            watchListView = new ListView
            {
                makeItem = () =>
                {
                    var item = new VisualElement();
                    item.style.flexDirection = FlexDirection.Row;
                    item.style.alignItems = Align.Center;
                    item.style.paddingLeft = 10;
                    item.style.paddingTop = 5;
                    item.style.paddingBottom = 5;

                    var dot = new VisualElement();
                    dot.style.width = 6;
                    dot.style.height = 6;
                    dot.style.borderTopLeftRadius = 3;
                    dot.style.borderTopRightRadius = 3;
                    dot.style.borderBottomLeftRadius = 3;
                    dot.style.borderBottomRightRadius = 3;
                    dot.style.backgroundColor = ColorSectionLive;
                    dot.style.marginRight = 8;
                    dot.name = "dot";

                    var label = new Label();
                    label.style.color = new Color(0.85f, 0.85f, 0.85f);
                    label.style.fontSize = 12;
                    label.name = "label";

                    item.Add(dot);
                    item.Add(label);
                    return item;
                },
                bindItem = (element, i) =>
                {
                    var label = element.Q<Label>("label");
                    if (label != null)
                        label.text = i < watchList.Count ? watchList[i].Label : string.Empty;
                },
                fixedItemHeight = 28,
                selectionType = SelectionType.Single,
            };
            watchListView.selectionChanged += OnWatchSelected;
            left.Add(watchListView);
            splitView.Add(left);
        }

        private void BuildRightPanel(TwoPaneSplitView splitView)
        {
            rightPanel = new ScrollView(ScrollViewMode.Vertical);
            rightPanel.style.paddingLeft = 12;
            rightPanel.style.paddingRight = 12;
            rightPanel.style.paddingTop = 8;
            splitView.Add(rightPanel);
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode) return;

            watchList.Clear();
            lastWatchCount = 0;
            selectedWatch = null;
            ClearRightPanel();
        }

        private void OnEditorUpdate()
        {
            if (!EditorApplication.isPlaying) return;

            var watches = CollectionDebug.GetWatches();

            if (watches.Count != lastWatchCount)
            {
                using (ListPool<ICollectionWatch>.Get(out var temp))
                {
                    foreach (var watch in watches.Values)
                        temp.Add(watch);

                    watchList.Clear();
                    foreach (var watch in temp)
                        watchList.Add(watch);
                }

                lastWatchCount = watches.Count;
                watchListView.itemsSource = watchList;
                watchListView.RefreshItems();

                if (selectedWatch == null && watchList.Count > 0)
                {
                    selectedWatch = watchList[0];
                    watchListView.selectedIndex = 0;
                    RebuildRightPanel();
                }
            }

            if (selectedWatch != null)
                UpdateRightPanel();
        }

        private void OnWatchSelected(IEnumerable<object> selection)
        {
            foreach (var item in selection)
            {
                selectedWatch = item as ICollectionWatch;
                break;
            }
            RebuildRightPanel();
        }

        private void RebuildRightPanel()
        {
            ClearRightPanel();
            if (selectedWatch == null) return;

            rightPanel.Add(BuildSectionHeader("LIVE", ColorSectionLive));

            liveFoldout = BuildFoldout(selectedWatch.Label, 0, ColorSectionLive, false);
            rightPanel.Add(liveFoldout);

            rightPanel.Add(BuildSectionHeader("SNAPSHOTS", ColorSectionSnap));

            var snapshots = CollectionDebug.GetSnapshot();
            foreach (var snapshot in snapshots)
            {
                if (snapshot.Label == selectedWatch.Label)
                    AddSnapshotFoldout(snapshot);
            }
        }

        private void UpdateRightPanel()
        {
            if (liveFoldout == null) return;

            var entries = selectedWatch.GetEntries();
            UpdateBadge(liveFoldout, entries.Length);
            liveFoldout.Clear();
            foreach (var entry in entries)
                liveFoldout.Add(BuildEntryRow(entry));

            var snapshots = CollectionDebug.GetSnapshot();

            int relevantCount = 0;
            foreach (var snapshot in snapshots)
            {
                if (snapshot.Label == selectedWatch.Label)
                    relevantCount++;
            }

            if (relevantCount == drawnSnapshots.Count) return;
            
            foreach (var snapshot in snapshots)
            {
                if (snapshot.Label == selectedWatch.Label && !drawnSnapshots.Contains(snapshot))
                    AddSnapshotFoldout(snapshot);
            }
        }

        private void AddSnapshotFoldout(SnapshotWatch snapshot)
        {
            var entries = snapshot.GetEntries();
            var foldout = BuildFoldout($"Snapshot {snapshot.timestamp}", entries.Length, ColorSectionSnap, false);

            foreach (var entry in entries)
                foldout.Add(BuildEntryRow(entry));

            rightPanel.Add(foldout);
            snapshotFoldouts.Add(foldout);
            drawnSnapshots.Add(snapshot);
        }

        private void ClearRightPanel()
        {
            rightPanel.Clear();
            liveFoldout = null;
            snapshotFoldouts.Clear();
            drawnSnapshots.Clear();
        }


        private static Foldout BuildFoldout(string label, int count, Color accentColor, bool expanded)
        {
            var foldout = new Foldout { value = expanded };
            foldout.style.marginBottom = 4;

            var header = foldout.Q<Toggle>();
            if (header != null)
            {
                header.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
                header.style.borderTopLeftRadius = 4;
                header.style.borderTopRightRadius = 4;
                header.style.paddingTop = 4;
                header.style.paddingBottom = 4;
            }

            var titleRow = new VisualElement();
            titleRow.style.flexDirection = FlexDirection.Row;
            titleRow.style.alignItems = Align.Center;
            titleRow.style.flexGrow = 1;

            var accent = new VisualElement();
            accent.style.width = 3;
            accent.style.height = 14;
            accent.style.backgroundColor = accentColor;
            accent.style.marginRight = 8;
            accent.style.borderTopLeftRadius = 2;
            accent.style.borderTopRightRadius = 2;
            accent.style.borderBottomLeftRadius = 2;
            accent.style.borderBottomRightRadius = 2;

            var title = new Label(label);
            title.style.color = new Color(0.9f, 0.9f, 0.9f);
            title.style.fontSize = 12;
            title.style.flexGrow = 1;
            title.name = "foldout-title";

            var badge = new Label(count.ToString());
            badge.name = "foldout-badge";
            badge.style.backgroundColor = ColorBadgeBg;
            badge.style.color = new Color(0.6f, 0.6f, 0.6f);
            badge.style.fontSize = 10;
            badge.style.paddingLeft = 6;
            badge.style.paddingRight = 6;
            badge.style.paddingTop = 2;
            badge.style.paddingBottom = 2;
            badge.style.borderTopLeftRadius = 8;
            badge.style.borderTopRightRadius = 8;
            badge.style.borderBottomLeftRadius = 8;
            badge.style.borderBottomRightRadius = 8;
            badge.style.marginRight = 8;

            titleRow.Add(accent);
            titleRow.Add(title);
            titleRow.Add(badge);
            foldout.text = string.Empty;
            foldout.Q<Toggle>()?.Add(titleRow);

            foldout.contentContainer.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            foldout.contentContainer.style.borderBottomLeftRadius = 4;
            foldout.contentContainer.style.borderBottomRightRadius = 4;
            foldout.contentContainer.style.paddingTop = 4;
            foldout.contentContainer.style.paddingBottom = 4;

            return foldout;
        }

        private static void UpdateBadge(Foldout foldout, int count)
        {
            var badge = foldout.Q<Label>("foldout-badge");
            if (badge != null)
                badge.text = count.ToString();
        }

        private static VisualElement BuildSectionHeader(string title, Color accentColor)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.marginTop = 12;
            container.style.marginBottom = 6;

            var label = new Label(title);
            label.style.fontSize = 10;
            label.style.color = accentColor;
            label.style.letterSpacing = 1.5f;
            label.style.marginRight = 8;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;

            var line = new VisualElement();
            line.style.height = 1;
            line.style.backgroundColor = ColorSeparator;
            line.style.flexGrow = 1;

            container.Add(label);
            container.Add(line);
            return container;
        }

        private static VisualElement BuildHorizontalSeparator()
        {
            var sep = new VisualElement();
            sep.style.height = 1;
            sep.style.backgroundColor = ColorSeparator;
            return sep;
        }

        private static VisualElement BuildEntryRow(WatchEntry entry)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.paddingLeft = 12;
            row.style.paddingTop = 3;
            row.style.paddingBottom = 3;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new Color(0.19f, 0.19f, 0.19f);

            var key = new Label(entry.key?.ToString() ?? "null");
            key.style.width = 90;
            key.style.color = new Color(0.85f, 0.85f, 0.85f);
            key.style.fontSize = 11;
            key.style.unityFontStyleAndWeight = FontStyle.Bold;

            var value = new Label(entry.value?.ToString() ?? "null");
            value.style.flexGrow = 1;
            value.style.color = new Color(0.85f, 0.85f, 0.85f);
            value.style.fontSize = 11;

            row.Add(key);
            row.Add(value);
            return row;
        }
    }
}