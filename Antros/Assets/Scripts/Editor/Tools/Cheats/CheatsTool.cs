using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ATCG.Debugging.Cheats;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.Cheats
{
    /// <summary>
    /// Editor tool that runs the game's cheats from the ATCG window. Providers are plain classes
    /// discovered by reflection (no scene components); each self-checks the runtime and exposes
    /// <see cref="CheatSection"/>s. The cheats are ALWAYS shown for discoverability — sections that
    /// can't run right now (not in Play mode, or no live context) are greyed out and non-interactive.
    /// Sections are the top-level boxes (e.g. "Player 1", "Player 2", "System"); inside, cheats are
    /// grouped by their <see cref="CheatGroupAttribute"/> and each cheat is its own card.
    /// </summary>
    public sealed class CheatsTool : IEditorTool
    {
        public string DisplayName => "Cheats";
        public string Icon => "⚡";
        public int Order => 70;

        private const double PollInterval = 0.5;

        private VisualElement content;
        private List<CheatProvider> providers;
        private double lastPoll;
        private string lastSignature;

        public VisualElement BuildUI()
        {
            VisualElement root = new VisualElement { style = { flexGrow = 1, minHeight = 0 } };
            EditorStyleLoader.Load(root, "EditorTheme.uss");
            EditorStyleLoader.Load(root, "Cheats.uss");

            Toolbar bar = new Toolbar();
            bar.Add(new Label("Cheats") { style = { unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft, marginLeft = 4 } });
            bar.Add(new ToolbarButton(Rebuild) { text = "Refresh" });
            root.Add(bar);

            ScrollView scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, minHeight = 0 } };
            content = new VisualElement();
            content.AddToClassList("cheat-content");
            scroll.Add(content);
            root.Add(scroll);

            Rebuild();
            return root;
        }

        public void OnActivated()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += OnUpdate;
            Rebuild();
        }

        public void OnDeactivated()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.update -= OnUpdate;
        }

        private void OnPlayModeChanged(PlayModeStateChange _) => Rebuild();

        // The battle/players usually aren't ready the instant Play starts, so a play-mode change
        // alone leaves the tool showing the disabled preview. Poll cheaply and rebuild only when the
        // availability actually changes (players connect/leave) — this keeps typed parameter values
        // while the context is stable, but activates the cheats as soon as the battle is live.
        private void OnUpdate()
        {
            if (EditorApplication.timeSinceStartup - lastPoll < PollInterval)
                return;
            lastPoll = EditorApplication.timeSinceStartup;

            if (ComputeSignature() != lastSignature)
                Rebuild();
        }

        private string ComputeSignature()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Application.isPlaying ? '1' : '0');

            foreach (CheatProvider provider in EnsureProviders())
            {
                sb.Append('|').Append(provider.GetType().Name).Append(':').Append(provider.IsAvailable ? '1' : '0');
                foreach (CheatSection section in provider.GetSections())
                    sb.Append('/').Append(section.Name).Append(section.Enabled ? '+' : '-');
            }

            return sb.ToString();
        }

        private void Rebuild()
        {
            if (content == null)
                return;

            content.Clear();

            bool playing = Application.isPlaying;
            if (!playing)
                content.Add(new HelpBox("Play mode is required to run cheats — shown disabled below.", HelpBoxMessageType.Info));

            bool anySection = false;
            foreach (CheatProvider provider in EnsureProviders())
            {
                bool providerOk = playing && provider.IsAvailable;
                List<CheatSection> sections = provider.GetSections()?.ToList() ?? new List<CheatSection>();
                if (sections.Count == 0)
                    continue;

                anySection = true;
                content.Add(sections.Count == 1
                    ? BuildSingleSection(sections[0], providerOk)
                    : BuildSectionTabs(sections, providerOk));
            }

            if (!anySection)
                content.Add(new HelpBox("No cheat providers found.", HelpBoxMessageType.Warning));

            lastPoll = EditorApplication.timeSinceStartup;
            lastSignature = ComputeSignature();
        }

        // One section → a titled rounded box.
        private VisualElement BuildSingleSection(CheatSection section, bool providerOk)
        {
            VisualElement box = new VisualElement();
            box.AddToClassList("cheat-section");

            Label title = new Label(section.Name);
            title.AddToClassList("cheat-section__title");
            box.Add(title);

            VisualElement body = BuildSectionBody(section);
            body.SetEnabled(providerOk && section.Enabled);
            box.Add(body);
            return box;
        }

        // Several sections from one provider → a rounded box holding a TabView (one tab per section,
        // e.g. "Player 1", "Player 2").
        private VisualElement BuildSectionTabs(List<CheatSection> sections, bool providerOk)
        {
            VisualElement box = new VisualElement();
            box.AddToClassList("cheat-section");

            TabView tabs = new TabView();
            tabs.AddToClassList("cheat-tabs");

            foreach (CheatSection section in sections)
            {
                Tab tab = new Tab(section.Name);
                VisualElement body = BuildSectionBody(section);
                body.SetEnabled(providerOk && section.Enabled);
                tab.Add(body);
                tabs.Add(tab);
            }

            box.Add(tabs);
            return box;
        }

        // The cheats of a section (grouped by [CheatGroup]), without the section chrome.
        private VisualElement BuildSectionBody(CheatSection section)
        {
            VisualElement body = new VisualElement();

            var groups = (section.Cheats ?? Enumerable.Empty<ICheat>())
                .Where(c => c != null)
                .GroupBy(GroupOf)
                .OrderBy(g => g.Key);

            bool any = false;
            foreach (var group in groups)
            {
                any = true;

                Label groupLabel = new Label(group.Key);
                groupLabel.AddToClassList("cheat-group__label");
                body.Add(groupLabel);

                foreach (ICheat cheat in group.OrderBy(c => c.Name))
                    body.Add(BuildCheatCard(cheat));
            }

            if (!any)
                body.Add(new Label("No cheats.") { style = { opacity = 0.6f } });

            return body;
        }

        // A cheat card: rounded box with name + Run on top, description, then aligned parameter
        // controls bound to this very instance (so the values the user sets persist until Execute).
        private VisualElement BuildCheatCard(ICheat cheat)
        {
            ICheat captured = cheat;

            VisualElement card = new VisualElement();
            card.AddToClassList("cheat-card");

            VisualElement head = new VisualElement();
            head.AddToClassList("cheat-card__head");

            Label name = new Label(cheat.Name);
            name.AddToClassList("cheat-card__title");
            head.Add(name);

            Button run = new Button(() => RunCheat(captured)) { text = "Run" };
            run.AddToClassList("cheat-card__run");
            head.Add(run);

            card.Add(head);

            if (!string.IsNullOrEmpty(cheat.Description))
            {
                Label desc = new Label(cheat.Description);
                desc.AddToClassList("cheat-card__desc");
                card.Add(desc);
            }

            VisualElement parameters = CheatParamsRenderer.Build(cheat);
            if (parameters != null)
            {
                parameters.AddToClassList("cheat-card__params");
                card.Add(parameters);
            }

            return card;
        }

        private static string GroupOf(ICheat cheat)
        {
            CheatGroupAttribute attr = cheat.GetType().GetCustomAttribute<CheatGroupAttribute>();
            return attr != null && !string.IsNullOrEmpty(attr.Group) ? attr.Group : "General";
        }

        private static async void RunCheat(ICheat cheat)
        {
            try
            {
                CheatContext context = new CheatContext { Picker = CheatChoicePopup.Show };
                await cheat.Execute(context);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        // Providers are stateless and don't change at runtime, so discover them once and reuse.
        private List<CheatProvider> EnsureProviders()
            => providers ??= DiscoverProviders().ToList();

        // All concrete CheatProviders with a public parameterless constructor, across loaded assemblies.
        private static IEnumerable<CheatProvider> DiscoverProviders()
        {
            List<CheatProvider> providers = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (Type type in types)
                {
                    if (type.IsAbstract || !typeof(CheatProvider).IsAssignableFrom(type))
                        continue;
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    try { providers.Add((CheatProvider)Activator.CreateInstance(type)); }
                    catch (Exception e) { Debug.LogWarning($"[Cheats] Couldn't create provider {type.Name}: {e.Message}"); }
                }
            }

            return providers;
        }
    }
}
