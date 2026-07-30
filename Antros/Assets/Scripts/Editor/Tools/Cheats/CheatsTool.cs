using System;
using System.Linq;
using System.Reflection;
using ATCG.Debugging.Cheats;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ATCG.Editor.Tools.Cheats
{
    /// <summary>
    /// Editor tool that runs the game's cheats from the ATCG window (Play mode only). It doesn't
    /// depend on any particular scene: it just asks the loaded scene(s) "who has cheats?" by
    /// finding every <see cref="CheatProvider"/>, then lists their cheats grouped by provider and
    /// by the cheat's <see cref="CheatGroupAttribute"/>. Running a cheat hands it a
    /// <see cref="CheatContext"/> whose picker is an editor popup (no in-game UI).
    /// </summary>
    public sealed class CheatsTool : IEditorTool
    {
        public string DisplayName => "Cheats";
        public string Icon => "⚡";
        public int Order => 70;

        private VisualElement content;

        public VisualElement BuildUI()
        {
            VisualElement root = new VisualElement { style = { flexGrow = 1, minHeight = 0 } };
            EditorStyleLoader.Load(root, "EditorTheme.uss");

            Toolbar bar = new Toolbar();
            Label title = new Label("Cheats") { style = { unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft, marginLeft = 4 } };
            bar.Add(title);
            bar.Add(new ToolbarButton(Rebuild) { text = "Refresh" });
            root.Add(bar);

            ScrollView scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, minHeight = 0 } };
            content = new VisualElement { style = { paddingTop = 4, paddingLeft = 4, paddingRight = 4, paddingBottom = 4 } };
            scroll.Add(content);
            root.Add(scroll);

            Rebuild();
            return root;
        }

        public void OnActivated()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            Rebuild();
        }

        public void OnDeactivated()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange _) => Rebuild();

        private void Rebuild()
        {
            if (content == null)
                return;

            content.Clear();

            if (!Application.isPlaying)
            {
                content.Add(new HelpBox("Enter Play mode to discover and run cheats.", HelpBoxMessageType.Info));
                return;
            }

            CheatProvider[] providers = Object.FindObjectsByType<CheatProvider>(FindObjectsSortMode.None);
            if (providers.Length == 0)
            {
                content.Add(new HelpBox("No cheat providers found in the loaded scene(s).", HelpBoxMessageType.Warning));
                return;
            }

            foreach (CheatProvider provider in providers.OrderBy(p => p.DisplayName))
                content.Add(BuildProvider(provider));
        }

        private VisualElement BuildProvider(CheatProvider provider)
        {
            Foldout providerFold = new Foldout { text = provider.DisplayName, value = true };
            providerFold.style.marginBottom = 6;
            providerFold.Q<Toggle>()?.AddToClassList("cheat-provider-toggle");

            var groups = provider.GetCheats()
                .Where(c => c != null)
                .GroupBy(GroupOf)
                .OrderBy(g => g.Key);

            bool any = false;
            foreach (var group in groups)
            {
                any = true;
                Foldout groupFold = new Foldout { text = group.Key, value = true };
                groupFold.style.marginLeft = 6;

                foreach (ICheat cheat in group.OrderBy(c => c.Name))
                    groupFold.Add(BuildCheatCard(cheat));

                providerFold.Add(groupFold);
            }

            if (!any)
                providerFold.Add(new Label("No cheats.") { style = { opacity = 0.6f, marginLeft = 6 } });

            return providerFold;
        }

        // A cheat row: name + Run on top, its parameter controls (if any) underneath. The param
        // controls bind to this very instance, so the values the user sets persist until Execute.
        private VisualElement BuildCheatCard(ICheat cheat)
        {
            ICheat captured = cheat;

            VisualElement card = new VisualElement { tooltip = cheat.Description, style = { marginTop = 2, marginBottom = 4 } };

            VisualElement head = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            head.Add(new Label(cheat.Name)
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft },
            });
            head.Add(new Button(() => RunCheat(captured)) { text = "Run" });
            card.Add(head);

            VisualElement parameters = CheatParamsRenderer.Build(cheat);
            if (parameters != null)
                card.Add(parameters);

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
    }
}
