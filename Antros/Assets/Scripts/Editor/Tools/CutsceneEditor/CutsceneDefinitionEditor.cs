using ATCG.Cutscenes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// Shared UI Toolkit inspector for non-capacity cutscene definitions: it draws the default fields
    /// and adds an "Edit Cutscene" button that opens the shared authoring stage on this asset (so its
    /// timeline / step markers can be authored in isolation). Capacities keep their own dedicated
    /// window and are intentionally not targeted here.
    ///
    /// A new cutscene kind gets the button by adding a one-line subclass with its own
    /// <c>[CustomEditor(typeof(TKind))]</c>, rather than targeting CutsceneDefinition with
    /// child-class inheritance (which would clash with the capacities' Odin inspector).
    /// </summary>
    public abstract class CutsceneDefinitionEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            EditorStyleLoader.Load(root, "EditorTheme.uss");
            EditorStyleLoader.Load(root, "Cutscenes.uss");

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            CutsceneDefinition definition = (CutsceneDefinition)target;

            Button edit = new(() => CutsceneAuthoring.Open(definition)) { text = "Edit Cutscene" };
            edit.AddToClassList("cutscene-edit-button");
            edit.SetEnabled(definition.Director != null);
            root.Add(edit);

            if (definition.Director == null)
            {
                root.Add(new HelpBox(
                    "Assign a Director prefab (with its Timeline) to author this cutscene.",
                    HelpBoxMessageType.Info));
            }

            return root;
        }
    }

    [CustomEditor(typeof(AttackCutscene))]
    public sealed class AttackCutsceneEditor : CutsceneDefinitionEditor { }
}
