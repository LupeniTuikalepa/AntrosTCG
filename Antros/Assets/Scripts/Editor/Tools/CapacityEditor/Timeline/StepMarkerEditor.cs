using System.Linq;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Capacities;
using UnityEditor;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Shows a step-name dropdown sourced from the currently-edited capacity's
    /// DeclaredSteps when a CapacityEditorTool session is active, so a marker can
    /// only reference a step that actually exists in code. Falls back to a plain
    /// text field with no active session (nothing to validate against).
    /// </summary>
    [CustomEditor(typeof(StepMarker))]
    public sealed class StepMarkerEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            SerializedProperty stepNameProp = serializedObject.FindProperty("stepName");

            CapacityData capacity = CapacityEditorTool.CurrentlyEdited;
            string[] declaredSteps = capacity != null ? CapacityEditorTool.GetDeclaredSteps(capacity) : null;

            if (declaredSteps == null || declaredSteps.Length == 0)
            {
                TextField field = new("Step Name") { value = stepNameProp.stringValue };
                field.RegisterValueChangedCallback(evt => ApplyStepName(stepNameProp, evt.newValue));
                root.Add(field);
                return root;
            }

            var choices = declaredSteps.ToList();
            int currentIndex = choices.IndexOf(stepNameProp.stringValue);

            // A freshly created marker has an empty stepName. The dropdown used to just
            // *display* index 0 in that case without writing it back, so the inspector
            // looked assigned to the first step while the serialized string stayed empty.
            // Default and persist it here so the display matches the actual data. A
            // non-empty value that simply isn't in the list (a since-removed step) is left
            // alone below, with a warning, instead of being silently overwritten.
            if (currentIndex < 0 && string.IsNullOrEmpty(stepNameProp.stringValue))
            {
                currentIndex = 0;
                ApplyStepName(stepNameProp, choices[0]);
            }

            DropdownField dropdown = new("Step", choices, currentIndex >= 0 ? currentIndex : 0);
            dropdown.RegisterValueChangedCallback(evt => ApplyStepName(stepNameProp, evt.newValue));
            root.Add(dropdown);

            if (currentIndex < 0 && !string.IsNullOrEmpty(stepNameProp.stringValue))
            {
                root.Add(new HelpBox(
                    $"'{stepNameProp.stringValue}' isn't a declared step anymore. Pick one from the list.",
                    HelpBoxMessageType.Warning));
            }

            return root;
        }

        private void ApplyStepName(SerializedProperty stepNameProp, string value)
        {
            stepNameProp.stringValue = value;
            serializedObject.ApplyModifiedProperties();
            EditorToolBus.Publish(new StepMarkerChangedEvent());
        }
    }
}
