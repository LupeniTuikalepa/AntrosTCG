using System.Collections.Generic;
using System.Linq;
using ATCG.Capacities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Add / remove a capacity's per-target tag constants. On Apply it rewrites the Data
    /// script's <c>[CapacityTargetTag] public const string NAME = nameof(NAME);</c>
    /// declarations via <see cref="CapacityTagEditor"/>, then a recompile happens. The base
    /// tags CELL/MEMBER (from <see cref="CapacityTags"/>) are always available and are not
    /// listed here.
    /// </summary>
    public sealed class EditTagsModal : EditorWindow
    {
        private CapacityData capacity;
        private readonly List<string> tags = new();
        private VisualElement listContainer;

        public static void Open(CapacityData capacity)
        {
            EditTagsModal window = CreateInstance<EditTagsModal>();
            window.capacity = capacity;
            window.titleContent = new GUIContent($"Edit Tags — {capacity.name}");

            Vector2 size = new Vector2(440, 400);
            window.minSize = new Vector2(380, 320);

            Rect main = EditorGUIUtility.GetMainWindowPosition();
            window.position = new Rect(main.center - size / 2f, size);

            window.ShowModalUtility();
        }

        private void CreateGUI()
        {
            tags.AddRange(CapacityTagEditor.ReadTags(capacity));

            VisualElement root = rootVisualElement;
            root.style.paddingTop = root.style.paddingBottom = 10;
            root.style.paddingLeft = root.style.paddingRight = 10;

            root.Add(new HelpBox(
                "Base tags CELL and MEMBER are always available via CapacityTags. Declare " +
                "extra tags here (emitted as [CapacityTargetTag] public const string NAME = " +
                "nameof(NAME);) to separate targets inside GetTargets, then query them in steps " +
                "with ctx.Targets.WithTags(...).",
                HelpBoxMessageType.Info));

            ScrollView scroll = new(ScrollViewMode.Vertical) { style = { flexGrow = 1, marginTop = 6 } };
            root.Add(scroll);

            scroll.Add(Title("Capacity tags"));
            listContainer = new VisualElement();
            scroll.Add(listContainer);
            RebuildList();

            scroll.Add(new Button(() => { tags.Add(string.Empty); RebuildList(); }) { text = "＋ Add tag" });

            Label error = new Label
            {
                style = { color = new Color(0.9f, 0.4f, 0.4f), whiteSpace = WhiteSpace.Normal, marginTop = 4, display = DisplayStyle.None }
            };
            root.Add(error);

            VisualElement buttons = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd, marginTop = 8 }
            };
            buttons.Add(new Button(Close) { text = "Cancel" });
            buttons.Add(new Button(() => Validate(error)) { text = "Apply" });
            root.Add(buttons);
        }

        private void RebuildList()
        {
            listContainer.Clear();

            if (tags.Count == 0)
                listContainer.Add(new Label("No extra tags — using only CELL / MEMBER.") { style = { opacity = 0.6f } });

            for (int i = 0; i < tags.Count; i++)
            {
                int index = i;
                VisualElement row = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 2 }
                };
                TextField field = new TextField { value = tags[index], style = { flexGrow = 1 } };
                field.RegisterValueChangedCallback(e => tags[index] = e.newValue);
                row.Add(field);
                row.Add(new Button(() => { tags.RemoveAt(index); RebuildList(); }) { text = "✕", style = { width = 22 } });
                listContainer.Add(row);
            }
        }

        private void Validate(Label error)
        {
            List<string> final = tags.Select(t => (t ?? string.Empty).Trim()).Where(t => t.Length > 0).ToList();

            foreach (string t in final)
            {
                if (!IsValidIdentifier(t))
                {
                    Fail(error, $"'{t}' is not a valid identifier.");
                    return;
                }
                if (t == CapacityTags.CELL || t == CapacityTags.MEMBER)
                {
                    Fail(error, $"'{t}' is a base tag (CapacityTags) — no need to redeclare it.");
                    return;
                }
            }

            if (final.Distinct().Count() != final.Count)
            {
                Fail(error, "Tag names must be unique.");
                return;
            }

            if (CapacityTagEditor.Apply(capacity, final, out string message))
                Close();
            else
                Fail(error, message);
        }

        private static void Fail(Label error, string message)
        {
            error.text = message;
            error.style.display = DisplayStyle.Flex;
        }

        private static Label Title(string text) => new Label(text)
        {
            style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 8, marginBottom = 2 }
        };

        private static bool IsValidIdentifier(string s)
        {
            if (string.IsNullOrEmpty(s) || (!char.IsLetter(s[0]) && s[0] != '_'))
                return false;
            for (int i = 1; i < s.Length; i++)
                if (!char.IsLetterOrDigit(s[i]) && s[i] != '_')
                    return false;
            return true;
        }
    }
}
