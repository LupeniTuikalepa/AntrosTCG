using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Cutscenes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Add / rename / remove a cutscene's steps. On validate it rewrites the class's [WithStep]
    /// attributes via CapacityStepEditor (and, for capacities only, the runtime logic's Execute{Step}
    /// methods), then a recompile happens.
    /// </summary>
    public sealed class EditStepsModal : EditorWindow
    {
        private sealed class Row
        {
            public string original;
            public int action; // 0 Ignore, 1 Rename, 2 Remove
            public string newName;
        }

        private CutsceneDefinition definition;
        private readonly List<Row> rows = new();
        private readonly List<string> added = new();
        private bool commentRemoved = true;

        private VisualElement addContainer;

        public static void Open(CutsceneDefinition definition)
        {
            EditStepsModal window = CreateInstance<EditStepsModal>();
            window.definition = definition;
            window.titleContent = new GUIContent($"Edit Steps — {definition.name}");

            Vector2 size = new Vector2(480, 440);
            window.minSize = new Vector2(420, 360);

            // Center on the main editor window (a modal utility otherwise pops top-left).
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            window.position = new Rect(main.center - size / 2f, size);

            window.ShowModalUtility();
        }

        private void CreateGUI()
        {
            foreach (string s in definition.DeclaredSteps)
                rows.Add(new Row { original = s, action = 0, newName = s });

            VisualElement root = rootVisualElement;
            root.style.paddingTop = root.style.paddingBottom = 10;
            root.style.paddingLeft = root.style.paddingRight = 10;

            ScrollView scroll = new(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
            root.Add(scroll);

            // --- Edit steps ---
            scroll.Add(Title("Edit steps"));
            if (rows.Count == 0)
                scroll.Add(new Label("No declared steps yet.") { style = { opacity = 0.6f } });
            foreach (Row row in rows)
                scroll.Add(BuildEditRow(row));

            // --- Add steps ---
            scroll.Add(Title("Add steps"));
            addContainer = new VisualElement();
            scroll.Add(addContainer);
            RebuildAdded();
            scroll.Add(new Button(() => { added.Add(string.Empty); RebuildAdded(); }) { text = "＋ Add step" });

            // --- options + actions ---
            Toggle commentToggle = new Toggle("Comment removed methods (else delete)") { value = commentRemoved };
            commentToggle.style.marginTop = 8;
            commentToggle.RegisterValueChangedCallback(e => commentRemoved = e.newValue);
            root.Add(commentToggle);

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

        private VisualElement BuildEditRow(Row row)
        {
            VisualElement box = new VisualElement
            {
                style = { marginBottom = 4, paddingTop = 4, paddingBottom = 4, paddingLeft = 6, paddingRight = 6,
                          backgroundColor = new Color(0, 0, 0, 0.15f), borderTopLeftRadius = 4, borderTopRightRadius = 4,
                          borderBottomLeftRadius = 4, borderBottomRightRadius = 4 }
            };

            box.Add(new Label(row.original) { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            TextField renameField = new TextField("New name")
            {
                value = row.newName,
                style = { display = row.action == 1 ? DisplayStyle.Flex : DisplayStyle.None, marginTop = 2 }
            };
            renameField.RegisterValueChangedCallback(e => row.newName = e.newValue);

            box.Add(BuildSegmented(new[] { "Ignore", "Rename", "Remove" }, row.action, idx =>
            {
                row.action = idx;
                renameField.style.display = idx == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            }));
            box.Add(renameField);
            return box;
        }

        // Segmented "toolbar toggle" group: horizontal, exactly one active at a time.
        private static VisualElement BuildSegmented(string[] labels, int current, Action<int> onChange)
        {
            Toolbar bar = new Toolbar { style = { marginTop = 2 } };
            ToolbarToggle[] toggles = new ToolbarToggle[labels.Length];

            for (int i = 0; i < labels.Length; i++)
            {
                int idx = i;
                ToolbarToggle toggle = new ToolbarToggle { text = labels[i], value = i == current };
                toggle.style.flexGrow = 1;
                toggle.style.unityTextAlign = TextAnchor.MiddleCenter;
                toggle.RegisterValueChangedCallback(e =>
                {
                    if (!e.newValue)
                    {
                        // Clicking the active segment shouldn't deselect it.
                        toggle.SetValueWithoutNotify(true);
                        return;
                    }

                    for (int j = 0; j < toggles.Length; j++)
                        if (j != idx)
                            toggles[j].SetValueWithoutNotify(false);

                    onChange(idx);
                });
                toggles[i] = toggle;
                bar.Add(toggle);
            }

            return bar;
        }

        private void RebuildAdded()
        {
            addContainer.Clear();
            for (int i = 0; i < added.Count; i++)
            {
                int index = i;
                VisualElement row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 2 } };
                TextField field = new TextField { value = added[index], style = { flexGrow = 1 } };
                field.RegisterValueChangedCallback(e => added[index] = e.newValue);
                row.Add(field);
                row.Add(new Button(() => { added.RemoveAt(index); RebuildAdded(); }) { text = "✕", style = { width = 22 } });
                addContainer.Add(row);
            }
        }

        private void Validate(Label error)
        {
            List<string> finalNames = new();
            foreach (Row r in rows)
            {
                if (r.action == 2)
                    continue;
                string n = r.action == 1 ? (r.newName ?? string.Empty).Trim() : r.original;
                if (r.action == 1 && !IsValidIdentifier(n))
                {
                    Fail(error, $"'{r.original}' rename target is not a valid identifier.");
                    return;
                }
                finalNames.Add(n);
            }
            foreach (string a in added.Select(s => (s ?? string.Empty).Trim()).Where(s => s.Length > 0))
            {
                if (!IsValidIdentifier(a))
                {
                    Fail(error, $"Added step '{a}' is not a valid identifier.");
                    return;
                }
                finalNames.Add(a);
            }
            if (finalNames.Distinct().Count() != finalNames.Count)
            {
                Fail(error, "Step names must be unique after the edits.");
                return;
            }

            List<CapacityStepEditor.Edit> edits = rows.Select(r => new CapacityStepEditor.Edit
            {
                original = r.original,
                action = (CapacityStepEditor.StepAction)r.action,
                newName = (r.newName ?? string.Empty).Trim(),
            }).ToList();

            if (CapacityStepEditor.Apply(definition, edits, added, commentRemoved, out string message))
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
