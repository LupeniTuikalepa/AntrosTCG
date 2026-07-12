using ATCG.Capacities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// One-shot builder for a capacity's cutscene stage. Creates the canonical folder
    /// layout, saves a PREFAB VARIANT of the shared template and a fresh TimelineAsset
    /// beside it, wires the timeline into the variant's PlayableDirector, and assigns
    /// that director back to the CapacityData. A variant (not a copy) keeps template
    /// changes flowing to every capacity's stage.
    /// </summary>
    public static class CapacityStageBuilder
    {
        public static bool TryBuild(CapacityData capacity, out string message)
        {
            if (capacity == null)
            {
                message = "No capacity selected.";
                return false;
            }

            CapacityEditorSettings settings = CapacityEditorSettings.GetOrCreate();
            GameObject template = settings.directorTemplate;
            if (template == null)
            {
                message = "No director template set (Settings tab).";
                return false;
            }

            // 1. Folders: Assets/Project/Capacities/{Element}/{CapacityName}/
            CapacityAssetLayout.EnsureCapacityFolder(capacity);
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(CapacityAssetLayout.DirectorPrefabPath(capacity));
            string timelinePath = AssetDatabase.GenerateUniqueAssetPath(CapacityAssetLayout.TimelinePath(capacity));

            // 2. Timeline asset.
            TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, timelinePath);

            // 3. Prefab variant of the template.
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(template);
            GameObject variant;
            try
            {
                variant = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out bool success);
                if (!success || variant == null)
                {
                    message = "Failed to save the prefab variant.";
                    return false;
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }

            // 4. Wire timeline into the variant's director (edit prefab contents so it
            //    persists on the asset, not on a throwaway instance).
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            bool saved;
            try
            {
                PlayableDirector director = root.GetComponentInChildren<PlayableDirector>();
                if (director == null)
                {
                    message = "Template has no PlayableDirector — fix the template and retry.";
                    return false;
                }

                director.playableAsset = timeline;
                saved = PrefabUtility.SavePrefabAsset(root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            if (!saved)
            {
                message = "Failed to save the timeline onto the prefab's PlayableDirector.";
                return false;
            }

            // Flush before reading the asset back — `variant` was obtained before this
            // second save to the same path, so its in-memory component references aren't
            // guaranteed to reflect what was just written. Reload from disk instead of
            // trusting `variant.GetComponentInChildren<PlayableDirector>()`.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject savedVariant = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            PlayableDirector assetDirector = savedVariant != null
                ? savedVariant.GetComponentInChildren<PlayableDirector>()
                : null;

            if (assetDirector == null)
            {
                message = "Prefab was saved but its PlayableDirector couldn't be reloaded — " +
                           "CutsceneDirector was left unassigned.";
                return false;
            }

            // 5. Assign the variant's director (on the saved asset) back to the data.
            AssignDirector(capacity, assetDirector);

            message = $"Built cutscene stage for '{capacity.name}' under " +
                      $"{System.IO.Path.GetDirectoryName(prefabPath)}.";
            return true;
        }

        // CutsceneDirector has a private setter; assign through SerializedObject.
        private static void AssignDirector(CapacityData capacity, PlayableDirector director)
        {
            SerializedObject so = new(capacity);
            SerializedProperty prop = so.FindProperty("<CutsceneDirector>k__BackingField");
            if (prop == null)
            {
                Debug.LogWarning("[CapacityTimelineEditor] No 'CutsceneDirector' field on CapacityData.");
                return;
            }
            prop.objectReferenceValue = director;
            so.ApplyModifiedProperties();
        }
    }
}