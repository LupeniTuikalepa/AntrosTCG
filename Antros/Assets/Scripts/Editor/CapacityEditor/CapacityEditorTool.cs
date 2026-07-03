using System;
using System.Reflection;
using ATCG.Capacities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Hub tool to author a capacity's cutscene: pick a CapacityData, open the debug
    /// scene, and setup/validate its timeline. Holds the "currently edited" context
    /// that the StepMarker dropdown reads (option A).
    /// </summary>
    public sealed class CapacityEditorTool : IEditorTool
    {
        private const string DebugScenePath = "Assets/Scenes/Editor/CapacitiesEdition.unity";

        // Context consumed by the StepMarker property drawer (option A).
        public static CapacityData CurrentlyEdited { get; private set; }

        public string DisplayName => "Capacity Editor";
        public string Icon => "d_Settings";
        public int Order => 50;

        private CapacityData selected;
        private ObjectField capacityField;
        private Label statusLabel;

        public VisualElement BuildUI()
        {
            VisualElement root = new();

            capacityField = new ObjectField("Capacity")
            {
                objectType = typeof(CapacityData),
                allowSceneObjects = false
            };
            capacityField.RegisterValueChangedCallback(OnCapacityChanged);
            root.Add(capacityField);

            Button openScene = new(OpenDebugScene) { text = "Open Debug Scene" };
            root.Add(openScene);

            statusLabel = new Label();
            root.Add(statusLabel);

            return root;
        }

        public void OnActivated() { }

        // Clear the shared context so a stale capacity doesn't leak to the drawer.
        public void OnDeactivated() => CurrentlyEdited = null;

        // Store selection and publish it as the edited context.
        private void OnCapacityChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            selected = evt.newValue as CapacityData;
            CurrentlyEdited = selected;
            statusLabel.text = selected != null ? $"Editing: {selected.name}" : string.Empty;
        }

        // Open the fixed debug scene (prompting to save the current one first).
        private void OpenDebugScene()
        {
            if (selected == null)
            {
                statusLabel.text = "Select a capacity first.";
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(DebugScenePath, OpenSceneMode.Single);
            statusLabel.text = $"Debug scene opened for {selected.name}.";
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