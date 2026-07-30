using System;
using System.Collections.Generic;
using System.Reflection;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks;
using ATCG.Capacities;
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

        private static CapacityData currentlyEdited;

        // Backed by the tool's selection, but falls back to the open cutscene stage's capacity: the
        // static field resets on a domain reload (recompile) while the stage survives, so anything
        // reading this (e.g. StepMarkerEditor's step dropdown) keeps working after a recompile.
        public static CapacityData CurrentlyEdited
        {
            get => currentlyEdited != null ? currentlyEdited : CapacityCutsceneStage.Current?.Capacity;
            private set => currentlyEdited = value;
        }

        public string DisplayName => "Capacity Editor";
        public string Icon => "⏱";
        public int Order => 50;

        private CapacityData selected;

        // Tabs
        private VisualElement authorTab;
        private VisualElement settingsTab;
        private Button authorTabButton;
        private Button settingsTabButton;

        // Author widgets
        private DropdownField capacityDropdown;
        private Label statusLabel;
        private Label directorStateLabel;
        private VisualElement stepsPanel;
        private VisualElement tracksPanel;
        private VisualElement propertiesPanel;
        private HelpBox warningsBox;
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
            authorTabButton = new Button(() => ShowTab(true)) { text = "Author" };
            settingsTabButton = new Button(() => ShowTab(false)) { text = "Settings" };
            authorTabButton.AddToClassList("ce-tab-btn");
            settingsTabButton.AddToClassList("ce-tab-btn");
            tabBar.Add(authorTabButton);
            tabBar.Add(settingsTabButton);
            root.Add(tabBar);

            authorTab = BuildAuthorTab();
            settingsTab = BuildSettingsTab();
            root.Add(authorTab);
            root.Add(settingsTab);

            ShowTab(true);
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

            Button newCapacityButton = new Button(NewCapacityModal.Open) { text = "＋ New Capacity" };
            newCapacityButton.style.marginBottom = 6;
            scroll.Add(newCapacityButton);

            VisualElement pickerRow = new();
            pickerRow.AddToClassList("ce-row");
            capacityDropdown = new DropdownField("Capacity");
            capacityDropdown.style.flexGrow = 1;
            capacityDropdown.RegisterValueChangedCallback(OnCapacityDropdownChanged);
            pickerRow.Add(capacityDropdown);
            pickerRow.Add(new Button(RefreshCatalog) { text = "↻" });
            pickerRow.Add(new Button(PingSelected) { text = "Ping" });
            scroll.Add(pickerRow);

            directorStateLabel = new Label();
            directorStateLabel.AddToClassList("ce-status");
            scroll.Add(directorStateLabel);

            buildStageButton = new Button(BuildStage) { text = "Create Cutscene Stage" };
            buildStageButton.style.display = DisplayStyle.None;
            scroll.Add(buildStageButton);

            VisualElement sceneRow = new();
            sceneRow.AddToClassList("ce-row");
            sceneRow.Add(new Button(EditCutscene) { text = "Edit Cutscene" });
            sceneRow.Add(new Button(RunScan) { text = "Rescan" });
            scroll.Add(sceneRow);

            statusLabel = new Label();
            statusLabel.AddToClassList("ce-status");
            scroll.Add(statusLabel);

            Label stepsTitle = new("Steps");
            stepsTitle.AddToClassList("ce-section-title");
            scroll.Add(stepsTitle);
            stepsPanel = new VisualElement();
            stepsPanel.AddToClassList("ce-steps-panel");
            scroll.Add(stepsPanel);

            Label tracksTitle = new("Auto-bindable Tracks");
            tracksTitle.AddToClassList("ce-section-title");
            scroll.Add(tracksTitle);
            tracksPanel = new VisualElement();
            tracksPanel.AddToClassList("ce-tracks-panel");
            scroll.Add(tracksPanel);

            Label propsTitle = new("Properties");
            propsTitle.AddToClassList("ce-section-title");
            scroll.Add(propsTitle);
            propertiesPanel = new VisualElement();
            propertiesPanel.AddToClassList("ce-props-panel");
            scroll.Add(propertiesPanel);

            warningsBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            warningsBox.AddToClassList("ce-warnings");
            warningsBox.style.display = DisplayStyle.None;
            scroll.Add(warningsBox);

            return tab;
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

        private void ShowTab(bool author)
        {
            authorTab.style.display = author ? DisplayStyle.Flex : DisplayStyle.None;
            settingsTab.style.display = author ? DisplayStyle.None : DisplayStyle.Flex;
            authorTabButton.EnableInClassList("ce-tab-btn--active", author);
            settingsTabButton.EnableInClassList("ce-tab-btn--active", !author);
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

            // After a domain reload our own 'selected' is null, but the cutscene stage
            // survives and knows its capacity — adopt it so the window doesn't fall back
            // to an empty picker while a stage is still open.
            if (selected == null && CapacityCutsceneStage.Current != null)
            {
                selected = CapacityCutsceneStage.Current.Capacity;
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
            OnSelectionChanged();
        }

        private void PingSelected()
        {
            if (selected != null)
                EditorGUIUtility.PingObject(selected);
        }

        private void OnSelectionChanged()
        {
            statusLabel.text = selected != null ? $"Editing: {selected.name}" : string.Empty;
            RefreshBuildButton();
            RefreshDirectorState();
            RebuildStepsPanel();
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

        private void RefreshDirectorState()
        {
            if (selected == null)
            {
                directorStateLabel.text = string.Empty;
                return;
            }

            bool hasDirector = selected.CutsceneDirector != null;
            bool hasTimeline = selected.CutsceneTimeline != null;
            directorStateLabel.text =
                $"Director: {(hasDirector ? "✓" : "✗")}   Timeline: {(hasTimeline ? "✓" : "✗")}";
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

            warningsBox.text = string.Join("\n", result.Warnings);
            warningsBox.style.display = result.Warnings.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnEditorUpdate()
        {
            if (selected == null)
                return;
            if (EditorApplication.timeSinceStartup - lastScanTime < ScanIntervalSeconds)
                return;

            lastScanTime = EditorApplication.timeSinceStartup;
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