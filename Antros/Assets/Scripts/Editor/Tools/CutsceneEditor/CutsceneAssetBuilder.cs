using System.IO;
using ATCG.Cutscenes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// One-shot builder that scaffolds a cutscene's stage next to its definition asset: a fresh
    /// TimelineAsset and a PREFAB VARIANT of the shared director template, wired together and assigned
    /// back onto the definition. A variant (not a copy) keeps template changes flowing to every
    /// cutscene's stage. This is the generic counterpart of the capacity stage builder — it locates
    /// the definition's director field by type, so it works for any cutscene kind regardless of how
    /// that field is named.
    /// </summary>
    public static class CutsceneAssetBuilder
    {
        public static bool TryBuild(CutsceneDefinition definition, string folder, string baseName, out string message)
        {
            if (definition == null)
            {
                message = "No definition to build.";
                return false;
            }

            CutsceneEditorSettings settings = CutsceneEditorSettings.GetOrCreate();
            GameObject template = settings.directorTemplate;
            if (template == null)
            {
                message = "No director template set (Cutscenes → Settings tab).";
                return false;
            }

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{baseName}Director.prefab");
            string timelinePath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{baseName}Timeline.playable");

            // 1. Timeline asset.
            TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, timelinePath);

            // 2. Prefab variant of the shared template. SaveAsPrefabAssetAndConnect is the API that
            // accepts a prefab INSTANCE (SaveAsPrefabAsset throws "Can't save a Prefab instance"); it
            // creates a variant of the template and returns it.
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(template);
            try
            {
                GameObject variant = PrefabUtility.SaveAsPrefabAssetAndConnect(
                    instance, prefabPath, InteractionMode.AutomatedAction);
                if (variant == null)
                {
                    message = "Failed to save the prefab variant.";
                    return false;
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }

            // 3. Wire the timeline into the variant's director (edit prefab contents so it persists on
            //    the asset, not on a throwaway instance).
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

            // Reload from disk before reading back — the earlier `variant` handle predates this save.
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject savedVariant = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            PlayableDirector assetDirector = savedVariant != null
                ? savedVariant.GetComponentInChildren<PlayableDirector>()
                : null;

            if (assetDirector == null)
            {
                message = "Prefab was saved but its PlayableDirector couldn't be reloaded — director left unassigned.";
                return false;
            }

            // 4. Assign the variant's director back onto the definition.
            if (!AssignDirector(definition, assetDirector))
            {
                message = "Couldn't find a PlayableDirector field on the definition to assign the stage to.";
                return false;
            }

            message = $"Built cutscene stage for '{definition.name}' under {folder}.";
            return true;
        }

        /// <summary>
        /// Repairs a definition that's missing its stage references, in place (next to the asset):
        /// a missing Director is fully rebuilt (director prefab variant + timeline), and a Director
        /// that lost its Timeline gets a fresh one assigned. A definition that's already complete is
        /// left untouched.
        /// </summary>
        public static bool TryFix(CutsceneDefinition definition, out string message)
        {
            if (definition == null)
            {
                message = "No definition to fix.";
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(definition);
            if (string.IsNullOrEmpty(assetPath))
            {
                message = "Definition isn't a saved asset.";
                return false;
            }

            string folder = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string baseName = definition.name;

            if (definition.Director == null)
                return TryBuild(definition, folder, baseName, out message);

            if (definition.Timeline == null)
                return TryAssignTimeline(definition, folder, baseName, out message);

            message = "Nothing to fix — the director and timeline are already set.";
            return false;
        }

        // Creates a fresh timeline and wires it into the definition's EXISTING director prefab (used
        // when the director is present but its timeline reference was lost).
        private static bool TryAssignTimeline(CutsceneDefinition definition, string folder, string baseName, out string message)
        {
            string prefabPath = AssetDatabase.GetAssetPath(definition.Director);
            if (string.IsNullOrEmpty(prefabPath))
            {
                message = "Director isn't a prefab asset — can't assign a timeline.";
                return false;
            }

            string timelinePath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{baseName}Timeline.playable");
            TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, timelinePath);

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            bool saved;
            try
            {
                PlayableDirector director = root.GetComponentInChildren<PlayableDirector>();
                if (director == null)
                {
                    message = "The director prefab has no PlayableDirector.";
                    return false;
                }

                director.playableAsset = timeline;
                saved = PrefabUtility.SavePrefabAsset(root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            message = saved
                ? $"Assigned a fresh timeline to '{definition.name}'."
                : "Failed to save the timeline onto the director prefab.";
            return saved;
        }

        // Finds the first serialized PlayableDirector reference on the definition (whatever its field
        // is named) and assigns the built director to it.
        private static bool AssignDirector(CutsceneDefinition definition, PlayableDirector director)
        {
            SerializedObject so = new(definition);
            SerializedProperty it = so.GetIterator();
            bool enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.type == "PPtr<$PlayableDirector>")
                {
                    it.objectReferenceValue = director;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(definition);
                    return true;
                }
            }
            return false;
        }
    }
}
