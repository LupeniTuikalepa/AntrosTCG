using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.Cheats
{
    /// <summary>
    /// Small non-modal editor popup that lets the user pick one option by name and resolves an
    /// <see cref="Awaitable{String}"/> with the choice (empty if cancelled). Used by the Cheats
    /// tool to fulfil a running cheat's target request without any in-game UI. Non-modal so the
    /// player loop keeps running while the cheat awaits the result.
    /// </summary>
    public sealed class CheatChoicePopup : EditorWindow
    {
        private AwaitableCompletionSource<string> source;
        private List<string> options;
        private readonly List<string> filtered = new();
        private ListView list;
        private bool resolved;

        public static Awaitable<string> Show(string title, IReadOnlyList<string> options)
        {
            CheatChoicePopup window = CreateInstance<CheatChoicePopup>();
            window.titleContent = new GUIContent(string.IsNullOrEmpty(title) ? "Pick a target" : title);
            window.options = options != null ? options.ToList() : new List<string>();
            window.source = new AwaitableCompletionSource<string>();

            Vector2 size = new Vector2(320, 380);
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            window.position = new Rect(main.center - size / 2f, size);
            window.minSize = new Vector2(240, 200);
            window.ShowUtility();

            return window.source.Awaitable;
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingTop = root.style.paddingBottom = 8;
            root.style.paddingLeft = root.style.paddingRight = 8;

            filtered.Clear();
            filtered.AddRange(options);

            ToolbarSearchField search = new ToolbarSearchField { style = { marginBottom = 6 } };
            search.RegisterValueChangedCallback(e => ApplyFilter(e.newValue));
            root.Add(search);

            list = new ListView(filtered, 20, () => new Label { style = { paddingLeft = 4, unityTextAlign = TextAnchor.MiddleLeft } },
                (element, i) => ((Label)element).text = filtered[i])
            {
                selectionType = SelectionType.Single,
                style = { flexGrow = 1, minHeight = 0 },
            };
            list.onItemsChosen += chosen => // double-click / Enter
            {
                if (chosen.FirstOrDefault() is string s)
                    Resolve(s);
            };
            root.Add(list);

            VisualElement buttons = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 6 }
            };
            buttons.Add(new Button(() => Resolve(string.Empty)) { text = "Cancel" });
            buttons.Add(new Button(() =>
            {
                if (list.selectedItem is string s)
                    Resolve(s);
            }) { text = "Choose" });
            root.Add(buttons);

            if (filtered.Count > 0)
                list.selectedIndex = 0;
        }

        private void ApplyFilter(string query)
        {
            filtered.Clear();
            if (string.IsNullOrEmpty(query))
                filtered.AddRange(options);
            else
                filtered.AddRange(options.Where(o => o.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0));

            list.RefreshItems();
            if (filtered.Count > 0)
                list.selectedIndex = 0;
        }

        private void Resolve(string value)
        {
            if (resolved)
                return;
            resolved = true;
            source.SetResult(value ?? string.Empty);
            Close();
        }

        // Closing the window any other way still resolves (as a cancel) so the awaiting cheat
        // never hangs.
        private void OnDestroy()
        {
            if (resolved)
                return;
            resolved = true;
            source.SetResult(string.Empty);
        }
    }
}
