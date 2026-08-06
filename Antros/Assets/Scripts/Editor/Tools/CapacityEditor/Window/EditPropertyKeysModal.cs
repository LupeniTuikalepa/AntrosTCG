using System.Collections.Generic;
using System.Linq;
using ATCG.Capacities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Add / remove a capacity's injected-property keys. On Apply it rewrites the runtime logic
    /// struct's <c>[CapacityPropertyKey] public const string KEY_PROPERTY = "KEY";</c> declarations
    /// via <see cref="CapacityPropertyKeyEditor"/>, then a recompile happens. Reference the keys in
    /// steps via <c>KEY_PROPERTY</c> (e.g. <c>ctx.capacityPhase.InjectProperty(KEY_PROPERTY, value)</c>).
    /// </summary>
    public sealed class EditPropertyKeysModal : EditorWindow
    {
        private CapacityData capacity;
        private readonly List<string> keys = new();
        private VisualElement listContainer;

        public static void Open(CapacityData capacity)
        {
            EditPropertyKeysModal window = CreateInstance<EditPropertyKeysModal>();
            window.capacity = capacity;
            window.titleContent = new GUIContent($"Edit Property Keys — {capacity.name}");

            Vector2 size = new Vector2(460, 400);
            window.minSize = new Vector2(380, 320);

            Rect main = EditorGUIUtility.GetMainWindowPosition();
            window.position = new Rect(main.center - size / 2f, size);

            window.ShowModalUtility();
        }

        private void CreateGUI()
        {
            keys.AddRange(CapacityPropertyKeyEditor.ReadKeys(capacity));

            VisualElement root = rootVisualElement;
            root.style.paddingTop = root.style.paddingBottom = 10;
            root.style.paddingLeft = root.style.paddingRight = 10;

            root.Add(new HelpBox(
                "Injected-property keys live on the runtime logic struct. A key NAME is emitted as " +
                "[CapacityPropertyKey] public const string NAME_PROPERTY = \"NAME\"; — reference it in " +
                "steps via NAME_PROPERTY with InjectProperty / TryGetProperty.",
                HelpBoxMessageType.Info));

            ScrollView scroll = new(ScrollViewMode.Vertical) { style = { flexGrow = 1, marginTop = 6 } };
            root.Add(scroll);

            scroll.Add(Title("Property keys"));
            listContainer = new VisualElement();
            scroll.Add(listContainer);
            RebuildList();

            scroll.Add(new Button(() => { keys.Add(string.Empty); RebuildList(); }) { text = "＋ Add key" });

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

            if (keys.Count == 0)
                listContainer.Add(new Label("No property keys yet.") { style = { opacity = 0.6f } });

            for (int i = 0; i < keys.Count; i++)
            {
                int index = i;
                VisualElement row = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 2 }
                };
                TextField field = new TextField { value = keys[index], style = { flexGrow = 1 } };
                field.RegisterValueChangedCallback(e => keys[index] = e.newValue);
                row.Add(field);
                row.Add(new Button(() => { keys.RemoveAt(index); RebuildList(); }) { text = "✕", style = { width = 22 } });
                listContainer.Add(row);
            }
        }

        private void Validate(Label error)
        {
            List<string> final = keys.Select(k => (k ?? string.Empty).Trim()).Where(k => k.Length > 0).ToList();

            foreach (string key in final)
            {
                if (!IsValidIdentifier(key))
                {
                    Fail(error, $"'{key}' is not a valid identifier.");
                    return;
                }
            }

            if (final.Distinct().Count() != final.Count)
            {
                Fail(error, "Property keys must be unique.");
                return;
            }

            if (CapacityPropertyKeyEditor.Apply(capacity, final, out string message))
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
