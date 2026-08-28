using System.Linq;
using ATCG.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Capacities;
using ATCG.Editor.Tools.CutsceneEditor;
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

            // Source the step names from the cutscene open in the authoring stage, so the dropdown works
            // for any cutscene kind (attacks, deploys, capacities) — all expose DeclaredSteps uniformly.
            CutsceneDefinition definition = CutsceneStage.Current?.Definition;
            string[] declaredSteps = definition?.DeclaredSteps.ToArray();

            if (declaredSteps == null || declaredSteps.Length == 0)
            {
                TextField field = new("Step Name") { value = stepNameProp.stringValue };
                field.RegisterValueChangedCallback(evt => ApplyStepName(stepNameProp, evt.newValue));
                root.Add(field);
                return root;
            }

            var choices = declaredSteps.ToList();
            int currentIndex = choices.IndexOf(stepNameProp.stringValue);

            // A freshly created marker has an empty stepName, or an existing marker can be
            // pointing at a step that's since been renamed/removed. Either way the dropdown
            // used to just *display* index 0 without writing it back — the normal Inspector
            // looked assigned to the first declared step while the serialized string stayed
            // empty/stale underneath (only visible by switching to Debug mode). Default and
            // persist to the first declared step here so the data actually matches what's
            // shown, instead of leaving a dangling reference behind a correct-looking label.
            string previousValue = stepNameProp.stringValue;
            bool wasInvalid = currentIndex < 0;
            if (wasInvalid)
            {
                currentIndex = 0;
                ApplyStepName(stepNameProp, choices[0]);
            }

            DropdownField dropdown = new("Step", choices, currentIndex);
            dropdown.RegisterValueChangedCallback(evt => ApplyStepName(stepNameProp, evt.newValue));
            root.Add(dropdown);

            if (wasInvalid && !string.IsNullOrEmpty(previousValue))
            {
                root.Add(new HelpBox(
                    $"'{previousValue}' wasn't a declared step anymore — reset to '{choices[0]}'.",
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
