using System;
using System.IO;
using ATCG.Cutscenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// Scaffolds and repairs a cutscene's stage assets next to its definition: the director prefab
    /// (a VARIANT of the shared template) and a TimelineAsset wired into it, then assigns the director
    /// onto the definition.
    ///
    /// Wiring the timeline is always done ASSET-SIDE (SerializedObject on the prefab's PlayableDirector
    /// component + AssetDatabase.SaveAssets) — never via LoadPrefabContents/SavePrefabAsset, because a
    /// variant's loaded contents are seen as a prefab instance and SavePrefabAsset then throws
    /// "Can't save a Prefab instance". The only step that needs a prefab INSTANCE is creating a brand
    /// new variant (SaveAsPrefabAsset only produces a variant from an instance, and instances only live
    /// in scenes) — that instance is placed in a throwaway regular scene and torn down immediately.
    /// Partial results are cleaned up on failure so nothing piles up.
    /// </summary>
    public static class CutsceneAssetBuilder
    {
        /// <summary>
        /// Optional per-type override for where a definition's stage assets (director prefab + timeline)
        /// belong, for kinds whose definition asset lives apart from its stage — e.g. capacities, whose
        /// data sits in Resources/Database but whose stage must live under Project/Cutscenes. Registered
        /// by that kind's editor code so this generic builder needs no dependency back onto it; a
        /// null/empty result falls back to the definition's own folder.
        /// </summary>
        public static Func<CutsceneDefinition, string> StageFolderResolver;

        /// <summary>Builds the director prefab variant + timeline for a definition that has none, in
        /// the given folder, and assigns the director back onto it.</summary>
        public static bool TryBuild(CutsceneDefinition definition, string folder, string baseName, out string message)
        {
            if (definition == null)
            {
                message = "No definition to build.";
                return false;
            }

            GameObject template = CutsceneEditorSettings.GetOrCreate().directorTemplate;
            if (template == null)
            {
                message = "No director template set (Cutscenes → Settings tab).";
                return false;
            }

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{baseName}Director.prefab");

            try
            {
                // 1. Create the director prefab VARIANT of the template.
                if (!TryCreateVariant(template, prefabPath, out PlayableDirector director, out message))
                {
                    AssetDatabase.DeleteAsset(prefabPath);
                    return false;
                }

                // 2. Timeline (next to the director) wired in asset-side (no scene, no instance).
                TryCreateTimelineNextTo(director, baseName, out TimelineAsset timeline);
                SetTimelineOnDirectorAsset(director, timeline);

                // 3. Assign the director onto the definition.
                if (!AssignDirector(definition, director))
                {
                    message = "Couldn't find a PlayableDirector field on the definition to assign the stage to.";
                    return false;
                }

                AssetDatabase.SaveAssets();
                message = $"Built cutscene stage for '{definition.name}' under {folder}.";
                return true;
            }
            catch (Exception e)
            {
                AssetDatabase.DeleteAsset(prefabPath);
                message = $"Build failed: {e.Message}";
                return false;
            }
        }

        /// <summary>
        /// Repairs a definition missing its stage references, next to the asset: a missing Director is
        /// fully rebuilt; a Director that lost its Timeline gets a fresh one wired into its existing
        /// prefab. A complete definition is left untouched.
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

            string baseName = definition.name;

            // A missing director is rebuilt into the definition's canonical STAGE folder (under
            // Project/Cutscenes) — never the definition's own folder, which for some kinds (capacities)
            // is Resources/Database. A missing timeline keys off the existing director instead.
            if (definition.Director == null)
                return TryBuild(definition, ResolveStageFolder(definition, assetPath), baseName, out message);

            if (definition.Timeline == null)
                return TryWireTimelineIntoExistingDirector(definition, baseName, out message);

            message = "Nothing to fix — the director and timeline are already set.";
            return false;
        }

        // Where a definition's stage assets belong: the registered resolver (e.g. capacities →
        // Project/Cutscenes/Capacities/...) if it answers, else the definition's own folder (correct
        // for kinds whose definition already lives beside its stage, like attack cutscenes).
        private static string ResolveStageFolder(CutsceneDefinition definition, string assetPath)
        {
            string resolved = StageFolderResolver?.Invoke(definition);
            return string.IsNullOrEmpty(resolved)
                ? Path.GetDirectoryName(assetPath).Replace('\\', '/')
                : resolved.Replace('\\', '/');
        }

        // Director present but timeline missing: create a timeline NEXT TO THE DIRECTOR PREFAB (not the
        // definition — they can live in different folders) and wire it in asset-side (no scene, no
        // instance — this preserves the variant).
        private static bool TryWireTimelineIntoExistingDirector(
            CutsceneDefinition definition, string baseName, out string message)
        {
            PlayableDirector director = definition.Director;
            if (!TryCreateTimelineNextTo(director, baseName, out TimelineAsset timeline))
            {
                message = "Director isn't a prefab asset — can't assign a timeline.";
                return false;
            }

            SetTimelineOnDirectorAsset(director, timeline);
            AssetDatabase.SaveAssets();

            message = $"Assigned a fresh timeline to '{definition.name}'.";
            return true;
        }

        // Creates a fresh TimelineAsset in the same folder as the director prefab, so a director and
        // its timeline always sit together regardless of where the definition asset lives.
        private static bool TryCreateTimelineNextTo(PlayableDirector director, string baseName, out TimelineAsset timeline)
        {
            timeline = null;
            string directorPath = AssetDatabase.GetAssetPath(director);
            if (string.IsNullOrEmpty(directorPath))
                return false;

            string folder = Path.GetDirectoryName(directorPath).Replace('\\', '/');
            string timelinePath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{baseName}Timeline.playable");
            timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, timelinePath);
            return true;
        }

        // Creates a prefab VARIANT of the template. A variant can only be produced from a prefab
        // instance (SaveAsPrefabAsset), and instances only exist in scenes — so we use a throwaway
        // regular scene (never a preview scene, which would make the save throw "Can't save a Prefab
        // instance") and tear it down immediately. Returns the reloaded director from the saved asset.
        private static bool TryCreateVariant(GameObject template, string prefabPath, out PlayableDirector director, out string message)
        {
            director = null;

            Scene temp = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject variant;
            bool success;
            try
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(template, temp);
                variant = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out success);
                UnityEngine.Object.DestroyImmediate(instance);
            }
            finally
            {
                EditorSceneManager.CloseScene(temp, removeScene: true);
            }

            if (!success || variant == null)
            {
                message = "Failed to save the director prefab variant.";
                return false;
            }

            director = variant.GetComponentInChildren<PlayableDirector>();
            if (director == null)
            {
                message = "The saved director prefab has no PlayableDirector.";
                return false;
            }

            message = null;
            return true;
        }

        // Sets a director prefab's timeline directly on the ASSET component — no scene, no instance,
        // no PrefabUtility.SavePrefabAsset (which rejects a variant's contents as an instance). On a
        // variant this simply records a property override. Caller flushes with AssetDatabase.SaveAssets.
        private static void SetTimelineOnDirectorAsset(PlayableDirector director, TimelineAsset timeline)
        {
            SerializedObject so = new(director);
            so.FindProperty("m_PlayableAsset").objectReferenceValue = timeline;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(director);
        }

        // Finds the first serialized PlayableDirector reference on the definition (whatever its field
        // is named) and assigns the director to it.
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
