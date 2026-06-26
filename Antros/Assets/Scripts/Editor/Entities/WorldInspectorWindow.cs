using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Runtime inspector for the custom ECS World.
    ///
    /// Shell responsibilities only: world picker, tab switching (Entities / Grid),
    /// the refresh model (manual snapshot by default, optional auto-refresh), and
    /// play-mode gating. The actual views live in EntitiesTabView and GridTabView.
    ///
    /// Refresh model: a snapshot is taken on demand (Snapshot button) or, if the user
    /// opts in, on a timer. Reading from a snapshot keeps the UI stable between refreshes
    /// — notably, component foldouts no longer snap back open under a periodic rebuild.
    /// </summary>
    public sealed class WorldInspectorWindow : EditorWindow
    {
        private const long AutoRefreshMs = 250;
        private const string ThemeUss = "EditorTheme.uss";
        private const string WindowUss = "WorldInspector.uss";

        private readonly WorldSnapshot snapshot = new();
        private readonly ComponentCatalog componentCatalog = new();
        private readonly AspectCatalog aspectCatalog = new();

        private EntitiesTabView entitiesTab;
        private GridTabView gridTab;

        private World selectedWorld;
        private bool autoRefresh;

        private PopupField<World> worldPicker;
        private Toggle autoToggle;
        private Button snapshotButton;
        private Label statusLabel;

        private VisualElement tabBar;
        private VisualElement tabContent;
        private VisualElement playModeOverlay;
        private int activeTab;

        private IVisualElementScheduledItem autoTask;

        [MenuItem("ATCG/World Inspector")]
        public static void Open()
        {
            WorldInspectorWindow wnd = GetWindow<WorldInspectorWindow>();
            wnd.titleContent = new GUIContent("World Inspector");
            wnd.minSize = new Vector2(520, 360);
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;

            AddSheet(root, ThemeUss);
            AddSheet(root, WindowUss);

            BuildToolbar(root);
            BuildTabBar(root);

            tabContent = new VisualElement();
            tabContent.AddToClassList("wi-tab-content");
            tabContent.style.flexGrow = 1;
            root.Add(tabContent);

            entitiesTab = new EntitiesTabView(componentCatalog, aspectCatalog);
            gridTab = new GridTabView();

            playModeOverlay = BuildPlayModeOverlay();
            root.Add(playModeOverlay);

            SelectTab(0);
            UpdateGate();
        }

        private void BuildToolbar(VisualElement root)
        {
            VisualElement toolbar = new();
            toolbar.AddToClassList("wi-topbar");

            worldPicker = new PopupField<World>(new List<World>(), 0, FormatWorld, FormatWorld);
            worldPicker.AddToClassList("wi-world-picker");
            worldPicker.RegisterValueChangedCallback(evt =>
            {
                selectedWorld = evt.newValue;
                TakeSnapshot();
            });
            toolbar.Add(worldPicker);

            snapshotButton = new Button(TakeSnapshot) { text = "Snapshot" };
            snapshotButton.AddToClassList("wi-snapshot-btn");
            toolbar.Add(snapshotButton);

            autoToggle = new Toggle("Auto") { value = false };
            autoToggle.AddToClassList("wi-auto-toggle");
            autoToggle.RegisterValueChangedCallback(evt => SetAutoRefresh(evt.newValue));
            toolbar.Add(autoToggle);

            statusLabel = new Label();
            statusLabel.AddToClassList("wi-status");
            toolbar.Add(statusLabel);

            root.Add(toolbar);
        }

        private void BuildTabBar(VisualElement root)
        {
            tabBar = new VisualElement();
            tabBar.AddToClassList("wi-tabbar");

            tabBar.Add(MakeTab("Entities", 0));
            tabBar.Add(MakeTab("Grid", 1));

            root.Add(tabBar);
        }

        private Button MakeTab(string label, int index)
        {
            Button tab = new(() => SelectTab(index)) { text = label };
            tab.AddToClassList("wi-tab-btn");
            tab.userData = index;
            return tab;
        }

        private void SelectTab(int index)
        {
            activeTab = index;

            for (int i = 0; i < tabBar.childCount; i++)
                tabBar[i].EnableInClassList("wi-tab-btn--active", (int)tabBar[i].userData == index);

            tabContent.Clear();
            tabContent.Add(index == 0 ? entitiesTab.Build() : gridTab.Build());
            PushDataToTabs();
        }

        private VisualElement BuildPlayModeOverlay()
        {
            VisualElement overlay = new();
            overlay.AddToClassList("wi-overlay");
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;

            Label msg = new("Enter play mode to inspect a world.");
            msg.AddToClassList("wi-overlay__msg");
            overlay.Add(msg);
            return overlay;
        }

        private void OnPlayModeChanged(PlayModeStateChange change)
        {
            // Clear on exit; refresh worlds on enter.
            if (change == PlayModeStateChange.ExitingPlayMode || change == PlayModeStateChange.EnteredEditMode)
            {
                selectedWorld = null;
                snapshot.Clear();
                componentCatalog.Rebuild();
            }
            UpdateGate();
            if (Application.isPlaying)
                TakeSnapshot();
        }

        private void UpdateGate()
        {
            bool playing = Application.isPlaying;

            if (playModeOverlay != null)
                playModeOverlay.style.display = playing ? DisplayStyle.None : DisplayStyle.Flex;

            SetEnabledDeep(playing);

            if (!playing)
            {
                statusLabel.text = "Not in play mode.";
                SetAutoRefresh(false);
                autoToggle?.SetValueWithoutNotify(false);
            }
            else
            {
                RefreshWorlds();
            }
        }

        private void SetEnabledDeep(bool enabled)
        {
            worldPicker?.SetEnabled(enabled);
            snapshotButton?.SetEnabled(enabled);
            autoToggle?.SetEnabled(enabled);
            tabBar?.SetEnabled(enabled);
            tabContent?.SetEnabled(enabled);
        }

        private void SetAutoRefresh(bool on)
        {
            autoRefresh = on;
            autoTask?.Pause();

            if (on && Application.isPlaying)
                autoTask = rootVisualElement.schedule.Execute(TakeSnapshot).Every(AutoRefreshMs);
        }

        private void TakeSnapshot()
        {
            if (!Application.isPlaying)
                return;

            componentCatalog.Rebuild();
            aspectCatalog.Rebuild();
            RefreshWorlds();

            snapshot.Capture(selectedWorld);
            PushDataToTabs();

            statusLabel.text = selectedWorld != null
                ? $"Snapshot \u00b7 {snapshot.Count} entities"
                : "No active world.";
        }

        private void PushDataToTabs()
        {
            entitiesTab?.RefreshCatalogs();
            entitiesTab?.SetData(selectedWorld, snapshot);
            gridTab?.SetData(selectedWorld, snapshot);
        }

        private void RefreshWorlds()
        {
            var worlds = new List<World>(World.ActiveWorlds.Count);
            foreach (World w in World.ActiveWorlds)
            {
                if (w != null)
                    worlds.Add(w);
            }

            worldPicker.choices = worlds;

            if (selectedWorld == null || !worlds.Contains(selectedWorld))
            {
                selectedWorld = worlds.Count > 0 ? worlds[0] : null;
                worldPicker.SetValueWithoutNotify(selectedWorld);
            }
        }

        private static string FormatWorld(World w)
        {
            if (w == null)
                return "<none>";
            try { return $"World ({w.Entities.Length} entities)"; }
            catch { return "World (?)"; }
        }

        private static void AddSheet(VisualElement root, string ussFileName)
        {
            string nameNoExt = System.IO.Path.GetFileNameWithoutExtension(ussFileName);
            foreach (string guid in AssetDatabase.FindAssets($"{nameNoExt} t:StyleSheet"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(ussFileName))
                    continue;
                StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (sheet != null)
                    root.styleSheets.Add(sheet);
                return;
            }
        }
    }
}
