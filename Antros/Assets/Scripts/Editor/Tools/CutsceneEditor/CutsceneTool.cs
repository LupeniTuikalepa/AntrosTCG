using System.Collections.Generic;
using System.Linq;
using ATCG.Capacities;
using ATCG.Cutscenes;
using ATCG.Editor.Tools.CapacityEditor; // capacity-specific authoring panels (property/tag/key/step editors + modals)
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// Hub tool for every cutscene definition — attacks, capacities, and any future kind. Three sub-tabs:
    ///   Explore  — search + a list grouped by broad kind (Physical Attacks, Capacities, …); "New"
    ///              scaffolds a fresh cutscene asset + its stage. Picking one sends it to Edition.
    ///   Edition  — everything to create/fix/edit a single cutscene: open its authoring stage and manage
    ///              its auto-bindable tracks.
    ///   Settings — the director template / test environment the creation flow and stage rely on.
    /// </summary>
    public sealed class CutsceneTool : IEditorTool
    {
        public string DisplayName => "Cutscenes";
        public string Icon => "▶";
        public int Order => 51;

        private enum Tab { Explore, Edition, Settings }

        // Broad grouping — one bucket per big kind rather than per concrete type, so the list doesn't
        // explode with one group per capacity subclass. Lower rank sorts higher.
        private static readonly (System.Type type, string label, int rank)[] Categories =
        {
            (typeof(AttackCutscene), "Physical Attacks", 0),
            (typeof(CapacityData), "Capacities", 1),
        };

        private VisualElement exploreTab;
        private VisualElement editionTab;
        private VisualElement settingsTab;
        private Button exploreButton;
        private Button editionButton;
        private Button settingsButton;

        private ScrollView list;
        private string filter = string.Empty;

        private CutsceneDefinition selected;
        private DropdownField editionPicker;
        private VisualElement editionBody;
        private readonly Dictionary<string, CutsceneDefinition> byLabel = new();

        private const double ScanIntervalSeconds = 0.5;
        private double lastScanTime;

        public VisualElement BuildUI()
        {
            VisualElement root = new();
            root.AddToClassList("cutscene-tool");
            EditorStyleLoader.Load(root, "EditorTheme.uss");
            EditorStyleLoader.Load(root, "Cutscenes.uss");

            Toolbar tabs = new();
            exploreButton = Tab_(tabs, "Explore", Tab.Explore);
            editionButton = Tab_(tabs, "Edition", Tab.Edition);
            settingsButton = Tab_(tabs, "Settings", Tab.Settings);
            root.Add(tabs);

            exploreTab = BuildExploreTab();
            editionTab = BuildEditionTab();
            settingsTab = BuildSettingsTab();
            root.Add(exploreTab);
            root.Add(editionTab);
            root.Add(settingsTab);

            ShowTab(Tab.Explore);
            return root;
        }

        private Button Tab_(Toolbar bar, string label, Tab tab)
        {
            Button b = new(() => ShowTab(tab)) { text = label };
            b.AddToClassList("cutscene-tab-button");
            bar.Add(b);
            return b;
        }

        public void OnActivated()
        {
            EditorToolBus.Subscribe<StepMarkerChangedEvent>(OnStepMarkerChanged);
            EditorApplication.update += OnEditorUpdate;
            Rebuild();
        }

        public void OnDeactivated()
        {
            EditorToolBus.Unsubscribe<StepMarkerChangedEvent>(OnStepMarkerChanged);
            EditorApplication.update -= OnEditorUpdate;
        }

        // ---- QTE-count scan (capacities only) -------------------------------

        // Periodically rescans the edited capacity's timeline and writes each step's QTE count back,
        // so the Steps panel stays in sync as QTE clips are added/moved. Migrated here from the old
        // capacity Author tab; only capacities have source-gen steps + QTE counts.
        private void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup - lastScanTime < ScanIntervalSeconds)
                return;

            lastScanTime = EditorApplication.timeSinceStartup;
            RunScanIfCapacity();
        }

        private void OnStepMarkerChanged(StepMarkerChangedEvent _) => RunScanIfCapacity();

        private void RunScanIfCapacity()
        {
            if (selected is not CapacityData capacity)
                return;

            TimelineAsset timeline = ResolveTimeline(capacity);
            if (timeline == null)
                return;

            string[] declaredSteps = capacity.DeclaredSteps.ToArray();
            CapacityTimelineScanner.Result result = CapacityTimelineScanner.Scan(timeline, declaredSteps);

            bool anyChanged = false;
            foreach (KeyValuePair<string, int> kv in result.QteCountByStep)
                anyChanged |= CapacityStepDataWriter.TrySetQteCount(capacity, kv.Key, kv.Value);

            if (!anyChanged)
                return;

            RebuildEditionBody();

            // Persist the write-back too if the stage for this capacity is open (respects Auto Save).
            CutsceneStage stage = CutsceneStage.Current;
            if (stage != null && stage.Definition == capacity)
                stage.AutoSaveIfEnabled();
        }

        // Reads from the open stage's live director (reflects unsaved edits) when it's editing this
        // cutscene; otherwise from the definition's saved timeline.
        private static TimelineAsset ResolveTimeline(CutsceneDefinition def)
        {
            CutsceneStage stage = CutsceneStage.Current;
            if (stage != null && stage.Definition == def && stage.Director != null)
                return stage.Director.playableAsset as TimelineAsset;
            return def.Timeline;
        }

        private void ShowTab(Tab tab)
        {
            exploreTab.EnableInClassList("hidden", tab != Tab.Explore);
            editionTab.EnableInClassList("hidden", tab != Tab.Edition);
            settingsTab.EnableInClassList("hidden", tab != Tab.Settings);
            exploreButton.EnableInClassList("cutscene-tab-button--active", tab == Tab.Explore);
            editionButton.EnableInClassList("cutscene-tab-button--active", tab == Tab.Edition);
            settingsButton.EnableInClassList("cutscene-tab-button--active", tab == Tab.Settings);

            if (tab == Tab.Explore)
                Rebuild();
            else if (tab == Tab.Edition)
                RebuildEdition();
        }

        // ---- Explore tab ----------------------------------------------------

        private VisualElement BuildExploreTab()
        {
            VisualElement tab = new();
            tab.AddToClassList("cutscene-tab");

            Toolbar bar = new();
            bar.Add(new ToolbarButton(() => NewCutsceneModal.Open(Rebuild)) { text = "New" });
            bar.Add(new ToolbarButton(Rebuild) { text = "Refresh" });

            ToolbarSearchField searchField = new();
            searchField.AddToClassList("cutscene-search");
            searchField.value = filter;
            searchField.RegisterValueChangedCallback(e =>
            {
                filter = e.newValue ?? string.Empty;
                Rebuild();
            });
            bar.Add(searchField);
            tab.Add(bar);

            list = new ScrollView(ScrollViewMode.Vertical);
            list.AddToClassList("cutscene-list");
            tab.Add(list);

            return tab;
        }

        private void Rebuild()
        {
            if (list == null)
                return;

            list.Clear();

            List<CutsceneDefinition> definitions = LoadAll();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                string needle = filter.Trim();
                definitions = definitions
                    .Where(d => d.name.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
            }

            if (definitions.Count == 0)
            {
                Label empty = new(string.IsNullOrWhiteSpace(filter)
                    ? "No cutscene definitions found in the project."
                    : $"No cutscene matches \"{filter}\".");
                empty.AddToClassList("cutscene-empty");
                list.Add(empty);
                return;
            }

            foreach (IGrouping<(int rank, string label), CutsceneDefinition> group in definitions
                         .GroupBy(Category)
                         .OrderBy(g => g.Key.rank).ThenBy(g => g.Key.label))
            {
                Label header = new(group.Key.label);
                header.AddToClassList("cutscene-group-header");
                list.Add(header);

                foreach (CutsceneDefinition definition in group.OrderBy(d => d.name))
                    list.Add(BuildRow(definition));
            }
        }

        private VisualElement BuildRow(CutsceneDefinition definition)
        {
            VisualElement row = new();
            row.AddToClassList("cutscene-row");

            Label name = new(definition.name);
            name.AddToClassList("cutscene-row-name");
            row.Add(name);

            bool hasDirector = definition.Director != null;
            bool hasTimeline = hasDirector && definition.Timeline != null;

            Label state = new(!hasDirector ? "no director" : !hasTimeline ? "no timeline" : "");
            state.AddToClassList("cutscene-row-state");
            row.Add(state);

            if (!hasDirector || !hasTimeline)
            {
                Button fix = new(() => { Fix(definition); Rebuild(); }) { text = "Fix" };
                fix.AddToClassList("cutscene-row-fix");
                row.Add(fix);
            }

            Button edit = new(() => SelectForEdition(definition)) { text = "Edit" };
            edit.AddToClassList("cutscene-row-edit");
            row.Add(edit);

            Button ping = new(() => EditorGUIUtility.PingObject(definition)) { text = "Ping" };
            ping.AddToClassList("cutscene-row-ping");
            row.Add(ping);

            return row;
        }

        private void SelectForEdition(CutsceneDefinition definition)
        {
            selected = definition;
            ShowTab(Tab.Edition);
        }

        // ---- Edition tab ----------------------------------------------------

        private VisualElement BuildEditionTab()
        {
            VisualElement tab = new();
            tab.AddToClassList("cutscene-tab");

            editionPicker = new DropdownField("Cutscene");
            editionPicker.RegisterValueChangedCallback(e =>
            {
                selected = byLabel.TryGetValue(e.newValue ?? string.Empty, out CutsceneDefinition d) ? d : null;
                RebuildEditionBody();
            });
            tab.Add(editionPicker);

            editionBody = new ScrollView(ScrollViewMode.Vertical);
            editionBody.AddToClassList("cutscene-list");
            tab.Add(editionBody);

            return tab;
        }

        private void RebuildEdition()
        {
            // After a recompile (triggered by editing steps/tags/keys) the domain reload drops the
            // tool-local selection; re-adopt the cutscene still open in the stage so the Edition tab
            // keeps showing it with freshly recompiled panels.
            selected ??= CutsceneStage.Current?.Definition;

            // Refresh the picker choices, keeping the current selection if still present.
            byLabel.Clear();
            List<string> labels = new();
            foreach (CutsceneDefinition d in LoadAll().OrderBy(d => Category(d).rank).ThenBy(d => d.name))
            {
                string label = $"{Category(d).label} / {d.name}";
                byLabel[label] = d;
                labels.Add(label);
            }

            editionPicker.choices = labels;
            string selectedLabel = selected != null ? byLabel.FirstOrDefault(kv => kv.Value == selected).Key : null;
            editionPicker.SetValueWithoutNotify(selectedLabel);

            RebuildEditionBody();
        }

        private void RebuildEditionBody()
        {
            editionBody.Clear();

            if (selected == null)
            {
                Label note = new("Pick a cutscene above (or hit Edit in Explore).");
                note.AddToClassList("cutscene-empty");
                editionBody.Add(note);
                return;
            }

            Label header = new($"{ObjectNames.NicifyVariableName(selected.GetType().Name)} — {selected.name}");
            header.AddToClassList("cutscene-group-header");
            editionBody.Add(header);

            bool hasDirector = selected.Director != null;
            bool hasTimeline = hasDirector && selected.Timeline != null;

            Button open = new(() => CutsceneAuthoring.Open(selected)) { text = "Open Stage" };
            open.AddToClassList("cutscene-edit-button");
            open.SetEnabled(hasDirector);
            editionBody.Add(open);

            // Something's missing → offer a one-click Fix that rebuilds the missing references
            // (director prefab variant + timeline, or just a fresh timeline) next to the asset.
            if (!hasDirector || !hasTimeline)
            {
                editionBody.Add(new HelpBox(
                    hasDirector
                        ? "The Director has no Timeline."
                        : "This cutscene has no Director/Timeline yet.",
                    HelpBoxMessageType.Warning));

                Button fix = new(FixSelected) { text = "Fix — rebuild missing references" };
                fix.AddToClassList("cutscene-edit-button");
                editionBody.Add(fix);
            }

            if (!hasDirector)
                return;

            editionBody.Add(BuildTracksPanel());

            // Steps are shared by every cutscene kind (declared via [WithStep]); this panel lists them
            // and lets you add / rename / remove from here.
            editionBody.Add(BuildStepsPanel(selected));

            // Remaining capacity-specific authoring (tags / property keys / properties) shown only when
            // the edited cutscene is a capacity.
            if (selected is CapacityData capacity)
                editionBody.Add(BuildCapacityPanels(capacity));
        }

        private VisualElement BuildStepsPanel(CutsceneDefinition definition)
        {
            VisualElement box = new();
            box.AddToClassList("cutscene-tracks-panel");

            box.Add(Section("Steps"));
            foreach (string step in definition.DeclaredSteps)
            {
                if (definition is CapacityData capacity && capacity.TryGetStep(step, out CapacityStepData data))
                    box.Add(new Label($"{step} — {data.QTEsCount} QTE(s)"));
                else
                    box.Add(new Label(step));
            }
            box.Add(new Button(() => EditStepsModal.Open(definition)) { text = "Edit steps" });
            return box;
        }

        private VisualElement BuildCapacityPanels(CapacityData capacity)
        {
            VisualElement box = new();
            box.AddToClassList("cutscene-tracks-panel");

            box.Add(Section("Properties"));
            box.Add(new CapacityPropertyEditor(capacity).Build());

            box.Add(Section("Target tags"));
            List<string> tags = CapacityTagEditor.ReadTags(capacity);
            if (tags.Count == 0)
                box.Add(new Label("Base tags only (CELL, MEMBER)."));
            foreach (string tag in tags)
                box.Add(new Label(tag));
            box.Add(new Button(() => EditTagsModal.Open(capacity)) { text = "Edit tags" });

            box.Add(Section("Property keys"));
            List<string> keys = CapacityPropertyKeyEditor.ReadKeys(capacity);
            if (keys.Count == 0)
                box.Add(new Label("No property keys."));
            foreach (string key in keys)
                box.Add(new Label(key));
            box.Add(new Button(() => EditPropertyKeysModal.Open(capacity)) { text = "Edit keys" });

            return box;
        }

        private static Label Section(string title)
        {
            Label l = new(title);
            l.AddToClassList("cutscene-group-header");
            return l;
        }

        private void FixSelected()
        {
            if (selected == null)
                return;

            Fix(selected);
            RebuildEdition();
        }

        // Rebuilds a definition's missing stage references (director / timeline) and logs the outcome.
        private static void Fix(CutsceneDefinition definition)
        {
            if (CutsceneAssetBuilder.TryFix(definition, out string message))
                Debug.Log($"[CutsceneEditor] {message}");
            else
                Debug.LogWarning($"[CutsceneEditor] Fix: {message}");
        }

        // The auto-bindable tracks panel: a checklist of the possible channels; ticking one adds the
        // track to the cutscene's timeline (bound to the open stage's rig if the stage is open).
        private VisualElement BuildTracksPanel()
        {
            VisualElement panel = new();
            panel.AddToClassList("cutscene-tracks-panel");

            Label title = new("Auto-bindable tracks");
            title.AddToClassList("cutscene-group-header");
            panel.Add(title);

            TimelineAsset timeline = selected.Timeline;
            if (timeline == null)
            {
                panel.Add(new HelpBox(
                    "The director has no TimelineAsset — open the stage once to scaffold it.",
                    HelpBoxMessageType.Info));
                return panel;
            }

            foreach (AutoBindChannel channel in CutsceneChannels.All)
            {
                bool present = CutsceneTimelineTrackBinder.HasTrack(timeline, channel);
                Toggle toggle = new(channel.displayName) { value = present };
                toggle.RegisterValueChangedCallback(e =>
                {
                    if (e.newValue)
                    {
                        (PlayableDirector director, DebugCutsceneRig rig) = StageRefsFor(selected);
                        CutsceneTimelineTrackBinder.AddTrack(timeline, channel, director, rig);
                    }
                    else
                    {
                        CutsceneTimelineTrackBinder.RemoveTrack(timeline, channel);
                    }
                    RebuildEditionBody();
                });
                panel.Add(toggle);
            }

            return panel;
        }

        // The live director + rig only exist while the stage for THIS cutscene is open; otherwise the
        // track is added unbound and gets bound on the next stage open.
        private static (PlayableDirector, DebugCutsceneRig) StageRefsFor(CutsceneDefinition definition)
        {
            CutsceneStage stage = CutsceneStage.Current;
            if (stage != null && stage.Definition == definition)
                return (stage.Director, stage.Rig);
            return (null, null);
        }

        // ---- Settings tab ---------------------------------------------------

        private VisualElement BuildSettingsTab()
        {
            VisualElement tab = new();
            tab.AddToClassList("cutscene-tab");

            CutsceneEditorSettings settings = CutsceneEditorSettings.GetOrCreate();

            Label heading = new("Templates used to scaffold and preview cutscenes.");
            heading.AddToClassList("cutscene-settings-note");
            tab.Add(heading);

            tab.Add(TemplateField("Director Template", settings.directorTemplate,
                v => { settings.directorTemplate = v as GameObject; settings.Save(); }));
            tab.Add(TemplateField("Test Environment", settings.testEnvironmentPrefab,
                v => { settings.testEnvironmentPrefab = v as GameObject; settings.Save(); }));

            ObjectField umotionField = new("UMotion Template")
            {
                objectType = typeof(Object),
                allowSceneObjects = false,
                value = settings.umotionTemplate
            };
            umotionField.RegisterValueChangedCallback(e => { settings.umotionTemplate = e.newValue; settings.Save(); });
            tab.Add(umotionField);

            Label shared = new("Single source of truth — the capacity tooling reads these same templates.");
            shared.AddToClassList("cutscene-settings-note");
            tab.Add(shared);

            return tab;
        }

        private static ObjectField TemplateField(string label, GameObject value, System.Action<Object> onChanged)
        {
            ObjectField field = new(label) { objectType = typeof(GameObject), allowSceneObjects = false, value = value };
            field.RegisterValueChangedCallback(e => onChanged(e.newValue));
            return field;
        }

        // ---- shared ---------------------------------------------------------

        private static (int rank, string label) Category(CutsceneDefinition definition)
        {
            foreach ((System.Type type, string label, int rank) in Categories)
                if (type.IsInstanceOfType(definition))
                    return (rank, label);

            return (int.MaxValue, ObjectNames.NicifyVariableName(definition.GetType().Name));
        }

        private static List<CutsceneDefinition> LoadAll()
        {
            return AssetDatabase.FindAssets("t:CutsceneDefinition")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<CutsceneDefinition>)
                .Where(d => d != null)
                .ToList();
        }
    }
}
