using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks;
using ATCG.Capacities;
using ATCG.Editor.Tools.DatabaseBrowser;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Hub tool to author a capacity's cutscene timeline. Two sub-tabs:
    ///   Author   — pick a capacity (grouped by Element), build its cutscene stage,
    ///              toggle auto-bindable channels, watch per-step QTE counts sync.
    ///   Settings — director template + editing-scene path (persisted in a versioned
    ///              CapacityEditorSettings asset).
    /// Holds the "currently edited" context StepMarkerEditor's dropdown reads (option A).
    /// </summary>
    public sealed class CapacityEditorTool : IEditorTool
    {
        private const double ScanIntervalSeconds = 0.5;
        private const string ThemeUss = "EditorTheme.uss";
        private const string ToolUss = "CapacityEditor.uss";
        private const string SelectedGuidKey = "ATCG.CapacityEditor.SelectedGuid";

        private static CapacityData currentlyEdited;

        // Backed by the tool's selection, but falls back to the open cutscene stage's capacity: the
        // static field resets on a domain reload (recompile) while the stage survives, so anything
        // reading this (e.g. StepMarkerEditor's step dropdown) keeps working after a recompile.
        public static CapacityData CurrentlyEdited
        {
            get => currentlyEdited != null ? currentlyEdited : CapacityCutsceneStage.Current?.Capacity;
            private set => currentlyEdited = value;
        }

        public string DisplayName => "Capacities";
        public string Icon => "⏱";
        public int Order => 50;

        private CapacityData selected;

        // Tabs
        private enum Tab { Edition, Assets, Settings }

        private VisualElement editionTab;
        private VisualElement assetsTab;
        private VisualElement settingsTab;
        private Button editionTabButton;
        private Button assetsTabButton;
        private Button settingsTabButton;

        private DatabaseBrowserView<CapacityData> assetsView;

        // Author widgets
        private DropdownField capacityDropdown;
        private Label statusLabel;
        private VisualElement liveDot;
        private Label statusCaption;
        private Label statusNameLabel;
        private VisualElement validityPanel;
        private VisualElement stepsPanel;
        private VisualElement tagsPanel;
        private VisualElement tracksPanel;
        private VisualElement propertiesPanel;
        private Button buildStageButton;

        // Flat lookup: dropdown label -> capacity (labels are "Element/Name").
        private readonly Dictionary<string, CapacityData> capacitiesByLabel = new();

        private double lastScanTime;

        public VisualElement BuildUI()
        {
            VisualElement root = new();
            root.AddToClassList("ce-root");
            root.style.flexGrow = 1;
            root.style.minHeight = 0;
            EditorStyleLoader.Load(root, ThemeUss);
            EditorStyleLoader.Load(root, ToolUss);

            VisualElement tabBar = new();
            tabBar.AddToClassList("ce-tabbar");
            editionTabButton = new Button(() => ShowTab(Tab.Edition)) { text = "Edition" };
            assetsTabButton = new Button(() => ShowTab(Tab.Assets)) { text = "Assets" };
            settingsTabButton = new Button(() => ShowTab(Tab.Settings)) { text = "Settings" };
            editionTabButton.AddToClassList("ce-tab-btn");
            assetsTabButton.AddToClassList("ce-tab-btn");
            settingsTabButton.AddToClassList("ce-tab-btn");
            tabBar.Add(editionTabButton);
            tabBar.Add(assetsTabButton);
            tabBar.Add(settingsTabButton);
            root.Add(tabBar);

            editionTab = BuildAuthorTab();
            assetsTab = BuildAssetsTab();
            settingsTab = BuildSettingsTab();
            root.Add(editionTab);
            root.Add(assetsTab);
            root.Add(settingsTab);

            ShowTab(Tab.Edition);
            return root;
        }

        // ---- Author tab ------------------------------------------------------

        private VisualElement BuildAuthorTab()
        {
            VisualElement tab = new();
            tab.AddToClassList("ce-tab");
            // flexGrow alone isn't enough here: Yoga flex items default to
            // min-height:auto (i.e. "never shrink below my content"), which is exactly
            // what was still forcing everything to cram instead of scroll. Zeroing it
            // lets this tab actually shrink to the space it's given, so the ScrollView
            // below gets a real bounded height to scroll within instead of just growing
            // past the window.
            tab.style.flexGrow = 1;
            tab.style.minHeight = 0;

            // All the actual content lives in a ScrollView so a cramped/docked window
            // scrolls instead of squishing every row down to nothing.
            ScrollView scroll = new(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.style.minHeight = 0;
            tab.Add(scroll);

            Label capacityTitle = new("Capacity");
            capacityTitle.AddToClassList("ce-section-title");
            scroll.Add(capacityTitle);

            // --- Header: selection, live status box, grouped actions ---------
            VisualElement header = new();
            header.AddToClassList("ce-header");

            Toolbar selectionBar = new();
            selectionBar.AddToClassList("ce-header-bar");
            capacityDropdown = new DropdownField();
            capacityDropdown.style.flexGrow = 1;
            capacityDropdown.RegisterValueChangedCallback(OnCapacityDropdownChanged);
            selectionBar.Add(capacityDropdown);
            selectionBar.Add(new ToolbarButton(RefreshCatalog) { text = "↻", tooltip = "Refresh catalog" });
            selectionBar.Add(new ToolbarButton(NewCapacityModal.Open) { text = "＋ New", tooltip = "Create a new capacity" });
            header.Add(selectionBar);

            // Live status box: a "live" dot + the capacity currently edited.
            VisualElement statusBox = new();
            statusBox.AddToClassList("ce-statusbox");
            liveDot = new VisualElement();
            liveDot.AddToClassList("ce-live-dot");
            statusCaption = new Label("EDITING");
            statusCaption.AddToClassList("ce-status-editing");
            statusNameLabel = new Label();
            statusNameLabel.AddToClassList("ce-status-name");
            statusBox.Add(liveDot);
            statusBox.Add(statusCaption);
            statusBox.Add(statusNameLabel);
            header.Add(statusBox);

            // Grouped action grid (wraps on narrow windows).
            VisualElement grid = new();
            grid.AddToClassList("ce-actions-grid");

            grid.Add(ActionGroup("Assets",
                Btn("Ping asset", PingSelected, "Ping the capacity asset in the Project window"),
                Btn("Ping folder", PingFolder, "Ping the capacity's asset + cutscene folder")));

            grid.Add(ActionGroup("Scripts",
                Btn("Data", OpenDataScript, "Open the Data script"),
                Btn("Battle", OpenLogicScript, "Open the runtime battle/logic script")));

            buildStageButton = Btn("Create Stage", BuildStage, "Create the cutscene stage");
            buildStageButton.style.display = DisplayStyle.None;
            grid.Add(ActionGroup("Cutscene",
                buildStageButton,
                Btn("Edit Cutscene", EditCutscene, "Open the cutscene stage"),
                Btn("Rescan", RunScan, "Rescan the timeline for step/QTE markers")));

            header.Add(grid);

            statusLabel = new Label();
            statusLabel.AddToClassList("ce-status");
            header.Add(statusLabel);

            scroll.Add(header);

            // --- Validity ----------------------------------------------------
            Label validityTitle = new("Validity");
            validityTitle.AddToClassList("ce-section-title");
            scroll.Add(validityTitle);
            ScrollView validityScroll = new(ScrollViewMode.Vertical);
            validityScroll.AddToClassList("ce-validity");
            // Lay the checks out in responsive columns; the ScrollView caps the height.
            validityScroll.contentContainer.style.flexDirection = FlexDirection.Row;
            validityScroll.contentContainer.style.flexWrap = Wrap.Wrap;
            validityPanel = validityScroll;
            scroll.Add(validityPanel);

            RefreshStatus();
            RefreshValidity();

            // --- Steps + Target Tags side by side (half width each) ----------
            VisualElement halfRow = new();
            halfRow.AddToClassList("ce-halfrow");

            VisualElement stepsCol = new();
            stepsCol.AddToClassList("ce-col");
            stepsCol.AddToClassList("ce-col--left");
            Label stepsTitle = new("Steps");
            stepsTitle.AddToClassList("ce-section-title");
            stepsCol.Add(stepsTitle);
            stepsPanel = new VisualElement();
            stepsPanel.AddToClassList("ce-steps-panel");
            stepsCol.Add(stepsPanel);
            halfRow.Add(stepsCol);

            VisualElement tagsCol = new();
            tagsCol.AddToClassList("ce-col");
            Label tagsTitle = new("Target Tags");
            tagsTitle.AddToClassList("ce-section-title");
            tagsCol.Add(tagsTitle);
            tagsPanel = new VisualElement();
            tagsPanel.AddToClassList("ce-steps-panel");
            tagsCol.Add(tagsPanel);
            halfRow.Add(tagsCol);

            scroll.Add(halfRow);

            // Auto-bindable Tracks (narrower) + Properties side by side.
            VisualElement lowerRow = new();
            lowerRow.AddToClassList("ce-halfrow");

            VisualElement tracksCol = new();
            tracksCol.AddToClassList("ce-col");
            tracksCol.AddToClassList("ce-col--left");
            tracksCol.AddToClassList("ce-col--narrow");
            Label tracksTitle = new("Auto-bindable Tracks");
            tracksTitle.AddToClassList("ce-section-title");
            tracksCol.Add(tracksTitle);
            tracksPanel = new VisualElement();
            tracksPanel.AddToClassList("ce-tracks-panel");
            tracksCol.Add(tracksPanel);
            lowerRow.Add(tracksCol);

            VisualElement propsCol = new();
            propsCol.AddToClassList("ce-col");
            propsCol.AddToClassList("ce-col--wide");
            Label propsTitle = new("Properties");
            propsTitle.AddToClassList("ce-section-title");
            propsCol.Add(propsTitle);
            propertiesPanel = new VisualElement();
            propertiesPanel.AddToClassList("ce-props-panel");
            propsCol.Add(propertiesPanel);
            lowerRow.Add(propsCol);

            scroll.Add(lowerRow);

            return tab;
        }

        // ---- Assets tab ------------------------------------------------------

        private VisualElement BuildAssetsTab()
        {
            VisualElement tab = new();
            tab.AddToClassList("ce-tab");
            tab.style.flexGrow = 1;
            tab.style.minHeight = 0;

            assetsView = new DatabaseBrowserView<CapacityData>(
                "Assets/Resources/Database/Capacities", "Capacities", SelectCapacity);

            VisualElement view = assetsView.Build();
            view.style.flexGrow = 1;
            view.style.minHeight = 0;
            tab.Add(view);
            return tab;
        }

        // Called by the Assets tab's Edit button: select the capacity and jump to Edition.
        private void SelectCapacity(CapacityData capacity)
        {
            if (capacity == null)
                return;

            string label = null;
            foreach (KeyValuePair<string, CapacityData> kv in capacitiesByLabel)
            {
                if (kv.Value == capacity)
                {
                    label = kv.Key;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(label))
            {
                // Setting the value fires OnCapacityDropdownChanged, which selects, persists
                // and rebuilds the Edition panels.
                capacityDropdown.value = label;
            }
            else
            {
                // Not in the catalog yet (e.g. just created): select it directly.
                selected = capacity;
                CurrentlyEdited = capacity;
                PersistSelected();
                OnSelectionChanged();
            }

            ShowTab(Tab.Edition);
        }

        private VisualElement BuildSettingsTab()
        {
            VisualElement tab = new();
            tab.AddToClassList("ce-tab");
            tab.style.flexGrow = 1;
            tab.style.minHeight = 0;

            ScrollView scroll = new(ScrollViewMode.Vertical);
            scroll.style.flexGrow = 1;
            scroll.style.minHeight = 0;
            tab.Add(scroll);

            CapacityEditorSettings settings = CapacityEditorSettings.GetOrCreate();

            Label title = new("Capacity Editor Settings");
            title.AddToClassList("ce-section-title");
            scroll.Add(title);

            ObjectField templateField = new("Director Template")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false,
                value = settings.directorTemplate
            };
            templateField.RegisterValueChangedCallback(evt =>
            {
                settings.directorTemplate = evt.newValue as GameObject;
                settings.Save();
            });
            scroll.Add(templateField);

            ObjectField envField = new("Test Environment")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false,
                value = settings.testEnvironmentPrefab
            };
            envField.RegisterValueChangedCallback(evt =>
            {
                settings.testEnvironmentPrefab = evt.newValue as GameObject;
                settings.Save();
            });
            scroll.Add(envField);

            ObjectField umotionField = new("UMotion Template")
            {
                objectType = typeof(UnityEngine.Object),
                allowSceneObjects = false,
                value = settings.umotionTemplate
            };
            umotionField.RegisterValueChangedCallback(evt =>
            {
                settings.umotionTemplate = evt.newValue;
                settings.Save();
            });
            scroll.Add(umotionField);

            VisualElement row = new();
            row.AddToClassList("ce-row");
            row.Add(new Button(() =>
            {
                CapacityEditorSettings s = CapacityEditorSettings.GetOrCreate();
                Selection.activeObject = s;
                EditorGUIUtility.PingObject(s);
            }) { text = "Ping Settings Asset" });
            scroll.Add(row);

            scroll.Add(new HelpBox(
                "Director Template: cloned as a prefab variant per capacity when you create a stage, " +
                "under Assets/Project/Capacities/{Element}/{Capacity}/.\n" +
                "Test Environment: hero + camera (CinemachineBrain) + DebugCutsceneRig, instantiated " +
                "inside the isolated cutscene stage so bindings preview correctly. It's scenery — never saved.",
                HelpBoxMessageType.Info));

            return tab;
        }

        private void ShowTab(Tab tab)
        {
            editionTab.style.display = tab == Tab.Edition ? DisplayStyle.Flex : DisplayStyle.None;
            assetsTab.style.display = tab == Tab.Assets ? DisplayStyle.Flex : DisplayStyle.None;
            settingsTab.style.display = tab == Tab.Settings ? DisplayStyle.Flex : DisplayStyle.None;
            editionTabButton.EnableInClassList("ce-tab-btn--active", tab == Tab.Edition);
            assetsTabButton.EnableInClassList("ce-tab-btn--active", tab == Tab.Assets);
            settingsTabButton.EnableInClassList("ce-tab-btn--active", tab == Tab.Settings);

            // Refresh the asset list when its tab is shown, so newly created capacities appear.
            if (tab == Tab.Assets)
                assetsView?.Reload();
        }

        // ---- lifecycle -------------------------------------------------------

        public void OnActivated()
        {
            EditorToolBus.Subscribe<StepMarkerChangedEvent>(OnStepMarkerChanged);
            EditorApplication.update += OnEditorUpdate;
            RefreshCatalog();

            // The stage may restore its Current slightly after this runs on a domain
            // reload; re-adopt it a tick later so the picker re-selects the edited
            // capacity even if Current wasn't ready yet when RefreshCatalog first ran.
            if (selected == null)
                EditorApplication.delayCall += RefreshCatalog;
        }

        public void OnDeactivated()
        {
            CurrentlyEdited = null;
            EditorToolBus.Unsubscribe<StepMarkerChangedEvent>(OnStepMarkerChanged);
            EditorApplication.update -= OnEditorUpdate;
        }

        // ---- capacity picker -------------------------------------------------

        private void RefreshCatalog()
        {
            capacitiesByLabel.Clear();
            List<string> choices = new();

            foreach (var group in CapacityCatalog.GroupedByElement())
            {
                foreach (CapacityData capacity in group.Value)
                {
                    string label = $"{group.Key}/{capacity.name}";
                    capacitiesByLabel[label] = capacity;
                    choices.Add(label);
                }
            }

            capacityDropdown.choices = choices;

            // After a domain reload our own 'selected' is null. Restore it from the persisted
            // selection first, then fall back to the open cutscene stage's capacity, so the picker
            // keeps its selection across recompiles.
            if (selected == null)
            {
                selected = RestoreSelected() ?? CapacityCutsceneStage.Current?.Capacity;
                if (selected != null)
                    CurrentlyEdited = selected;
            }

            if (selected != null)
            {
                string current = choices.Find(c => capacitiesByLabel[c] == selected);
                capacityDropdown.SetValueWithoutNotify(current);
                OnSelectionChanged();
            }
        }

        private void OnCapacityDropdownChanged(ChangeEvent<string> evt)
        {
            capacitiesByLabel.TryGetValue(evt.newValue ?? string.Empty, out selected);
            CurrentlyEdited = selected;
            PersistSelected();
            OnSelectionChanged();
        }

        private void PersistSelected()
        {
            string guid = selected != null
                ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selected))
                : null;

            if (string.IsNullOrEmpty(guid))
                SessionState.EraseString(SelectedGuidKey);
            else
                SessionState.SetString(SelectedGuidKey, guid);
        }

        private static CapacityData RestoreSelected()
        {
            string guid = SessionState.GetString(SelectedGuidKey, null);
            if (string.IsNullOrEmpty(guid))
                return null;
            return AssetDatabase.LoadAssetAtPath<CapacityData>(AssetDatabase.GUIDToAssetPath(guid));
        }

        private void PingSelected()
        {
            if (selected != null)
                EditorGUIUtility.PingObject(selected);
        }

        private void OnSelectionChanged()
        {
            statusLabel.text = string.Empty;
            RefreshStatus();
            RefreshValidity();
            RefreshBuildButton();
            RebuildStepsPanel();
            RebuildTagsPanel();
            RebuildTracksPanel();
            RebuildPropertiesPanel();
        }

        private void RebuildPropertiesPanel()
        {
            propertiesPanel.Clear();
            if (selected == null)
                return;
            propertiesPanel.Add(new CapacityPropertyEditor(selected).Build());
        }

        // Live status box: the dot is "live" (green) only while the cutscene stage is
        // actually open for the selection; otherwise it's idle and reads "SELECTED".
        private void RefreshStatus()
        {
            bool has = selected != null;
            bool live = has && IsStageOpenFor(selected);

            liveDot.EnableInClassList("ce-live-dot--idle", !live);
            statusCaption.text = has ? (live ? "EDITING" : "SELECTED") : "—";
            statusNameLabel.text = has ? selected.name : "No capacity selected";
            statusNameLabel.EnableInClassList("ce-status-name--idle", !has);
        }

        private static bool IsStageOpenFor(CapacityData capacity)
        {
            CapacityCutsceneStage stage = CapacityCutsceneStage.Current;
            return stage != null && stage.Capacity == capacity;
        }

        // ---- validity checks -------------------------------------------------

        private enum Severity { Ok, Warning, Critical }

        // Rebuilds the Validity list. Only FAILING checks are shown (passing ones are
        // hidden); blocking issues — those that would stop the capacity from being played
        // — render red, the rest orange. When everything passes, a single "ready" line
        // is shown. Called on selection change and after scans.
        private void RefreshValidity()
        {
            validityPanel.Clear();

            if (selected == null)
            {
                validityPanel.Add(new Label("No capacity selected.") { style = { opacity = 0.6f } });
                return;
            }

            string dataPath = CapacityStepEditor.LocateDataScript(selected);
            string logicPath = CapacityStepEditor.LocateLogicScript(selected);
            string[] steps = GetDeclaredSteps(selected);
            TimelineAsset timeline = ResolveTimeline();

            List<(string label, bool critical)> failing = new();
            void Check(string label, bool ok, bool critical)
            {
                if (!ok)
                    failing.Add((label, critical));
            }

            // Non-blocking (nice to fix, but the capacity can still run).
            Check("Display name missing", !string.IsNullOrEmpty(selected.Name), false);
            Check("Description missing", !string.IsNullOrEmpty(selected.Description), false);
            Check("No cast pattern defined", !selected.CastPatterns.IsEmpty, false);
            Check("Asset folder missing", AssetDatabase.IsValidFolder(CapacityAssetLayout.CapacityFolder(selected)), false);
            Check("Director prefab missing from folder", System.IO.File.Exists(CapacityAssetLayout.DirectorPrefabPath(selected)), false);
            Check("Timeline asset missing from folder", System.IO.File.Exists(CapacityAssetLayout.TimelinePath(selected)), false);

            // Blocking (would prevent the capacity from being played).
            Check("Null property definition(s)",
                selected.PropertyDefinitions == null || selected.PropertyDefinitions.All(p => p != null), true);
            Check("Data script not found", !string.IsNullOrEmpty(dataPath), true);
            Check("Battle script not found", !string.IsNullOrEmpty(logicPath), true);
            Check("No step declared", steps.Length > 0, true);
            Check("No cutscene director assigned", selected.CutsceneDirector != null, true);
            Check("No cutscene timeline assigned", selected.CutsceneTimeline != null, true);

            // Timeline markers — only meaningful once a timeline exists.
            if (timeline != null)
            {
                (HashSet<string> markerNames, bool hasUnnamed) = CollectMarkers(timeline);
                (bool ok, string label) markers = CheckStepMarkers(timeline, steps, markerNames);
                Check(markers.label, markers.ok, true);
                Check("Unnamed step marker(s) on the timeline", !hasUnnamed, false);
                Check("Step marker(s) reference an unknown step", markerNames.All(n => Array.IndexOf(steps, n) >= 0), false);
            }

            if (failing.Count == 0)
            {
                validityPanel.Add(CheckRow("The asset is ready to be used.", Severity.Ok));
                return;
            }

            // Blocking issues first, then warnings.
            foreach ((string label, bool critical) in failing.OrderByDescending(c => c.critical))
                validityPanel.Add(CheckRow(label, critical ? Severity.Critical : Severity.Warning));
        }

        private static VisualElement CheckRow(string label, Severity severity)
        {
            VisualElement row = new();
            row.AddToClassList("ce-check-row");
            row.Add(ValidityIcon(severity));
            Label text = new(label) { style = { unityTextAlign = TextAnchor.MiddleLeft } };
            text.EnableInClassList("ce-check--critical", severity == Severity.Critical);
            row.Add(text);
            return row;
        }

        // Round, filled Unity status light: green (ok) / orange (warning) / red (critical).
        // Falls back to a coloured dot if the built-in icon can't be resolved.
        private static VisualElement ValidityIcon(Severity severity)
        {
            string iconName = severity switch
            {
                Severity.Ok => "lightMeter/greenLight",
                Severity.Critical => "lightMeter/redLight",
                _ => "lightMeter/orangeLight",
            };

            Texture tex = EditorGUIUtility.IconContent(iconName)?.image;
            if (tex != null)
            {
                Image image = new Image { image = tex };
                image.style.width = 14;
                image.style.height = 14;
                image.style.marginRight = 6;
                image.style.flexShrink = 0;
                return image;
            }

            Color color = severity switch
            {
                Severity.Ok => new Color(0.46f, 0.78f, 0.51f),
                Severity.Critical => new Color(0.86f, 0.35f, 0.35f),
                _ => new Color(0.95f, 0.7f, 0.3f),
            };

            VisualElement dot = new();
            dot.style.width = 9;
            dot.style.height = 9;
            dot.style.marginRight = 6;
            dot.style.flexShrink = 0;
            dot.style.borderTopLeftRadius = dot.style.borderTopRightRadius = 9;
            dot.style.borderBottomLeftRadius = dot.style.borderBottomRightRadius = 9;
            dot.style.backgroundColor = color;
            return dot;
        }

        // Every declared step must have a matching, named StepMarker on the timeline.
        // Returns the problem statement (with the offending steps) when some are missing.
        private static (bool ok, string label) CheckStepMarkers(TimelineAsset timeline, string[] steps, HashSet<string> markerNames)
        {
            if (steps.Length == 0)
                return (true, string.Empty);

            List<string> missing = steps.Where(s => !markerNames.Contains(s)).ToList();
            return missing.Count == 0
                ? (true, string.Empty)
                : (false, $"Missing step marker(s): {string.Join(", ", missing)}");
        }

        // Collects the named StepMarkers on the timeline and whether any are unnamed.
        private static (HashSet<string> names, bool hasUnnamed) CollectMarkers(TimelineAsset timeline)
        {
            HashSet<string> names = new();
            bool hasUnnamed = false;

            if (timeline == null)
                return (names, false);

            void Process(TrackAsset track)
            {
                foreach (IMarker marker in track.GetMarkers())
                {
                    if (marker is not StepMarker stepMarker)
                        continue;
                    if (string.IsNullOrEmpty(stepMarker.StepName))
                        hasUnnamed = true;
                    else
                        names.Add(stepMarker.StepName);
                }
            }

            if (timeline.markerTrack != null)
                Process(timeline.markerTrack);
            foreach (TrackAsset track in timeline.GetOutputTracks())
                Process(track);

            return (names, hasUnnamed);
        }

        // Pings the capacity's asset + cutscene folder (Project/Capacities/{Element}/{name}).
        private void PingFolder()
        {
            if (selected == null)
            {
                statusLabel.text = "Select a capacity first.";
                return;
            }

            string folder = CapacityAssetLayout.CapacityFolder(selected);
            UnityEngine.Object folderObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folder);
            if (folderObj == null)
            {
                statusLabel.text = $"No asset folder yet for '{selected.name}'.";
                return;
            }

            Selection.activeObject = folderObj;
            EditorGUIUtility.PingObject(folderObj);
        }

        // ---- action grid helpers ---------------------------------------------

        private static Button Btn(string text, System.Action onClick, string tooltip)
        {
            Button button = new Button(onClick) { text = text, tooltip = tooltip };
            button.AddToClassList("ce-gbtn");
            return button;
        }

        private static VisualElement ActionGroup(string title, params VisualElement[] buttons)
        {
            VisualElement group = new();
            group.AddToClassList("ce-actiongroup");

            Label caption = new(title);
            caption.AddToClassList("ce-actiongroup__title");
            group.Add(caption);

            VisualElement row = new();
            row.AddToClassList("ce-actiongroup__row");
            foreach (VisualElement button in buttons)
                row.Add(button);
            group.Add(row);

            return group;
        }

        // Stage build is offered until both a director and its timeline exist.
        private void RefreshBuildButton()
        {
            bool needsStage = selected != null &&
                              (selected.CutsceneDirector == null || selected.CutsceneTimeline == null);
            buildStageButton.style.display = needsStage ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void BuildStage()
        {
            if (CapacityStageBuilder.TryBuild(selected, out string message))
                OnSelectionChanged();
            statusLabel.text = message;
        }

        // Opens the isolated cutscene stage (Prefab-Mode-like) for the selection.
        private void EditCutscene()
        {
            if (selected == null)
            {
                statusLabel.text = "Select a capacity first.";
                return;
            }
            if (selected.CutsceneDirector == null)
            {
                statusLabel.text = $"'{selected.name}' has no cutscene stage yet.";
                return;
            }

            CapacityCutsceneStage.Open(selected);
            statusLabel.text = $"Editing '{selected.name}' cutscene in an isolated stage.";
        }

        // ---- open scripts ----------------------------------------------------

        private void OpenDataScript()
        {
            if (selected == null)
            {
                statusLabel.text = "Select a capacity first.";
                return;
            }
            OpenScriptAt(CapacityStepEditor.LocateDataScript(selected), "Data");
        }

        private void OpenLogicScript()
        {
            if (selected == null)
            {
                statusLabel.text = "Select a capacity first.";
                return;
            }
            OpenScriptAt(CapacityStepEditor.LocateLogicScript(selected), "runtime logic");
        }

        private void OpenScriptAt(string path, string label)
        {
            if (string.IsNullOrEmpty(path))
            {
                statusLabel.text = $"Couldn't locate the {label} script.";
                return;
            }

            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null)
                AssetDatabase.OpenAsset(script);
            else
                statusLabel.text = $"Couldn't open the {label} script at {path}.";
        }

        // ---- steps / QTE counts ---------------------------------------------

        private void RebuildStepsPanel()
        {
            stepsPanel.Clear();
            if (selected == null)
                return;

            foreach (string step in GetDeclaredSteps(selected))
            {
                selected.TryGetStep(step, out CapacityStepData data);
                Label row = new($"{step} — {data.QTEsCount} QTE(s)");
                row.AddToClassList("ce-step-row");
                stepsPanel.Add(row);
            }

            Button editSteps = new Button(() => EditStepsModal.Open(selected)) { text = "Edit steps" };
            editSteps.style.marginTop = StyleKeyword.Auto; // pin to the bottom of the panel
            stepsPanel.Add(editSteps);
        }

        private void RebuildTagsPanel()
        {
            tagsPanel.Clear();
            if (selected == null)
                return;

            List<string> tags = CapacityTagEditor.ReadTags(selected);
            if (tags.Count == 0)
                tagsPanel.Add(new Label("Base tags only (CELL, MEMBER).") { style = { opacity = 0.6f } });

            foreach (string tag in tags)
            {
                Label row = new(tag);
                row.AddToClassList("ce-step-row");
                tagsPanel.Add(row);
            }

            Button editTags = new Button(() => EditTagsModal.Open(selected)) { text = "Edit tags" };
            editTags.style.marginTop = StyleKeyword.Auto; // pin to the bottom of the panel
            tagsPanel.Add(editTags);
        }

        // When the cutscene stage is open for this capacity, read the timeline from the
        // stage's live director (reflects unsaved edits); otherwise fall back to the
        // capacity's director prefab timeline.
        private TimelineAsset ResolveTimeline()
        {
            if (selected == null)
                return null;

            CapacityCutsceneStage stage = CapacityCutsceneStage.Current;
            if (stage != null && stage.Capacity == selected && stage.Director != null)
                return stage.Director.playableAsset as TimelineAsset;

            return selected.CutsceneTimeline;
        }

        private void RunScan()
        {
            if (selected == null)
                return;

            TimelineAsset timeline = ResolveTimeline();
            if (timeline == null)
                return;

            string[] declaredSteps = GetDeclaredSteps(selected);
            CapacityTimelineScanner.Result result = CapacityTimelineScanner.Scan(timeline, declaredSteps);

            bool anyChanged = false;
            foreach (var kv in result.QteCountByStep)
                anyChanged |= CapacityStepDataWriter.TrySetQteCount(selected, kv.Key, kv.Value);

            if (anyChanged)
            {
                RebuildStepsPanel();

                // If the cutscene stage is open for this capacity, persist the scan
                // write-back too (respecting the Auto Save toggle).
                CapacityCutsceneStage stage = CapacityCutsceneStage.Current;
                if (stage != null && stage.Capacity == selected)
                    stage.AutoSaveIfEnabled();
            }

            RefreshValidity();
        }

        private void OnEditorUpdate()
        {
            if (selected == null)
                return;
            if (EditorApplication.timeSinceStartup - lastScanTime < ScanIntervalSeconds)
                return;

            lastScanTime = EditorApplication.timeSinceStartup;
            RefreshStatus(); // reflect the cutscene stage being opened/closed
            RunScan();
        }

        private void OnStepMarkerChanged(StepMarkerChangedEvent evt) => RunScan();

        // ---- auto-bindable tracks checklist ----------------------------------

        private void RebuildTracksPanel()
        {
            tracksPanel.Clear();

            TimelineAsset timeline = ResolveTimeline();
            if (timeline == null)
            {
                tracksPanel.Add(new Label("No timeline yet — create a stage to manage tracks."));
                return;
            }

            CapacityCutsceneStage stage = CapacityCutsceneStage.Current;
            PlayableDirector sceneDirector = stage != null ? stage.Director : null;
            DebugCutsceneRig rig = stage != null ? stage.Rig : null;

            foreach (AutoBindChannel channel in CutsceneChannels.All)
            {
                AutoBindChannel captured = channel;
                Toggle toggle = new(channel.displayName)
                {
                    value = CapacityTimelineTrackBinder.HasTrack(timeline, channel)
                };
                toggle.AddToClassList("ce-track-toggle");
                toggle.AddToClassList("atcg-toggle-accent");
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        CapacityTimelineTrackBinder.AddTrack(timeline, captured, sceneDirector, rig);
                    else
                        CapacityTimelineTrackBinder.RemoveTrack(timeline, captured);

                    RebuildTracksPanel();
                });
                tracksPanel.Add(toggle);
            }
        }

        // Reads the generated `static string[] DeclaredSteps` off the concrete type.
        public static string[] GetDeclaredSteps(CapacityData capacity)
        {
            if (capacity == null)
                return Array.Empty<string>();

            FieldInfo field = capacity.GetType().GetField(
                "DeclaredSteps",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return field?.GetValue(null) as string[] ?? Array.Empty<string>();
        }
    }
}