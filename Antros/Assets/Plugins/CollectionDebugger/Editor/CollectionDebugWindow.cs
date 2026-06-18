using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CollectionDebugger.Core;

namespace CollectionDebugger.Editor
{
    public class CollectionDebugWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private readonly Dictionary<string, bool> foldouts = new();

        [MenuItem("Tools/Collection Debugger")]
        public static void Open() => GetWindow<CollectionDebugWindow>("Collection Debugger");

        private void OnEnable()
        {
            EditorApplication.update += Repaint;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to see watches.", MessageType.Info);
                return;
            }

            var watches = CollectionDebug.GetWatches();

            if (watches.Count == 0)
            {
                EditorGUILayout.HelpBox("No collections watched.", MessageType.Warning);
                return;
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var collection in watches)
            {
                DrawCollection(collection.Value);
            }

            GUILayout.EndScrollView();
        }

        private void DrawCollection(ICollectionWatch watch)
        {
            foldouts.TryAdd(watch.Label, false); 

            var entries = watch.GetEntries();

            foldouts[watch.Label] = EditorGUILayout.Foldout(
                foldouts[watch.Label],
                $"{watch.Label}  ({entries.Length})",
                true
            );

            if (!foldouts[watch.Label])
                return;

            EditorGUI.indentLevel++;

            var watchEntries = watch.GetEntries();
            for (var index = 0; index < watchEntries.Length; index++)
            {
                var entry = watchEntries[index];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{index} :", GUILayout.Width(40));
                EditorGUILayout.LabelField(entry.key?.ToString() ?? "null", GUILayout.Width(80));
                EditorGUILayout.LabelField(entry.value?.ToString() ?? "null");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }
    }
}