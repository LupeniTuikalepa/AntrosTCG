using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks;
using UnityEditor;
using UnityEngine;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Inspector for the DebugCutsceneRig. Adds a "Populate from CutsceneChannels"
    /// button that appends any auto-bindable channel missing from the table, without
    /// touching rows already filled. New channels declared in CutsceneChannels.All
    /// show up here on a re-click — no recompile of the rig component itself.
    /// </summary>
    [CustomEditor(typeof(DebugCutsceneRig))]
    public sealed class DebugCutsceneRigEditor : UnityEditor.Editor
    {
        private SerializedProperty bindingsProp;

        private void OnEnable()
        {
            bindingsProp = serializedObject.FindProperty("bindings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(bindingsProp, true);

            EditorGUILayout.Space();
            if (GUILayout.Button("Populate from CutsceneChannels"))
                Populate();

            serializedObject.ApplyModifiedProperties();
        }

        // Adds a row per CutsceneChannels.All entry not already present (matched by
        // channel name), leaving existing rows and their references untouched.
        private void Populate()
        {
            HashSet<string> existing = new();
            for (int i = 0; i < bindingsProp.arraySize; i++)
            {
                SerializedProperty element = bindingsProp.GetArrayElementAtIndex(i);
                existing.Add(element.FindPropertyRelative("channelName").stringValue);
            }

            foreach (AutoBindChannel channel in CutsceneChannels.All)
            {
                if (existing.Contains(channel.trackName))
                    continue;

                bindingsProp.arraySize++;
                SerializedProperty added = bindingsProp.GetArrayElementAtIndex(bindingsProp.arraySize - 1);
                added.FindPropertyRelative("channelName").stringValue = channel.trackName;
                added.FindPropertyRelative("reference").objectReferenceValue = null;
            }
        }
    }
}