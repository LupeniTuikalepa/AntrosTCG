using System;
using System.Collections.Generic;
using System.Linq;
using ATCG.Capacities;
using ATCG.Cutscenes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// Modal to create a new cutscene definition asset: pick the kind (any concrete non-capacity
    /// <see cref="CutsceneDefinition"/> — capacities have their own scaffolding flow), name it, and on
    /// Create it scaffolds the asset + its stage and opens the shared authoring stage on it.
    /// </summary>
    public sealed class NewCutsceneModal : EditorWindow
    {
        private readonly Dictionary<string, Type> typesByLabel = new();
        private string chosenLabel;
        private string cutsceneName = string.Empty;
        private Action onCreated;

        public static void Open(Action onCreated)
        {
            NewCutsceneModal window = CreateInstance<NewCutsceneModal>();
            window.onCreated = onCreated;
            window.titleContent = new GUIContent("New Cutscene");
            window.minSize = new Vector2(360, 190);
            window.ShowModalUtility();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            EditorStyleLoader.Load(root, "EditorTheme.uss");
            EditorStyleLoader.Load(root, "Cutscenes.uss");
            root.AddToClassList("cutscene-modal");

            Label title = new("Create a new cutscene");
            title.AddToClassList("cutscene-modal-title");
            root.Add(title);

            List<string> labels = BuildTypeTable();
            if (labels.Count == 0)
            {
                root.Add(new HelpBox(
                    "No concrete cutscene types found. Add a non-abstract CutsceneDefinition subclass first.",
                    HelpBoxMessageType.Warning));
                root.Add(new Button(Close) { text = "Close" });
                return;
            }

            chosenLabel = labels[0];
            DropdownField typeField = new("Type", labels, 0);
            typeField.RegisterValueChangedCallback(e => chosenLabel = e.newValue);
            root.Add(typeField);

            TextField nameField = new("Name") { value = cutsceneName };
            nameField.RegisterValueChangedCallback(e => cutsceneName = e.newValue);
            root.Add(nameField);

            Label error = new();
            error.AddToClassList("cutscene-modal-error");
            error.AddToClassList("hidden");
            root.Add(error);

            VisualElement buttons = new();
            buttons.AddToClassList("cutscene-modal-buttons");
            buttons.Add(new Button(Close) { text = "Cancel" });
            buttons.Add(new Button(() => TryCreate(error)) { text = "Create" });
            root.Add(buttons);

            nameField.Focus();
        }

        private void TryCreate(Label error)
        {
            Type type = typesByLabel.TryGetValue(chosenLabel ?? string.Empty, out Type t) ? t : null;

            if (CutsceneCreator.Create(type, cutsceneName, out CutsceneDefinition created, out string message))
            {
                onCreated?.Invoke();
                Close();
                // Defer only the stage opening until after this modal has fully closed, so the stage
                // doesn't try to take over the scene view mid-teardown.
                if (created != null && created.Director != null)
                    EditorApplication.delayCall += () => CutsceneAuthoring.Open(created);
            }
            else
            {
                error.text = message;
                error.RemoveFromClassList("hidden");
            }
        }

        private List<string> BuildTypeTable()
        {
            typesByLabel.Clear();
            List<string> labels = new();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<CutsceneDefinition>()
                         .Where(t => !t.IsAbstract && !typeof(CapacityData).IsAssignableFrom(t))
                         .OrderBy(t => t.Name))
            {
                string label = ObjectNames.NicifyVariableName(type.Name);
                typesByLabel[label] = type;
                labels.Add(label);
            }

            return labels;
        }
    }
}
