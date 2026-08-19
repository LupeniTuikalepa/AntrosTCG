using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Cutscenes;
using ATCG.Capacities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Instantiates a capacity's cutscene stage (the prefab hosting its
    /// PlayableDirector) into the current scene. The editing scene is reloaded fresh
    /// before each call (see CapacityDebugSceneUtility.Reopen), so there's nothing to
    /// clean up here and no marker component is needed.
    /// </summary>
    public static class CapacityStageInstantiator
    {
        public static CapacityCutscene LoadStage(CapacityData capacity)
        {
            if (capacity == null || capacity.Director == null)
                return null;

            GameObject prefabRoot = ResolvePrefabRoot(capacity.Director);
            if (prefabRoot == null)
            {
                Debug.LogWarning(
                    $"[CapacityTimelineEditor] Couldn't resolve the prefab for '{capacity.name}'. " +
                    $"Director must reference a PlayableDirector that lives inside a prefab asset.");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
            if (instance == null)
            {
                Debug.LogWarning(
                    $"[CapacityTimelineEditor] InstantiatePrefab returned null for '{prefabRoot.name}'.");
                return null;
            }

            EditorSceneManager.MarkSceneDirty(instance.scene);
            return instance.GetComponentInChildren<CapacityCutscene>();
        }

        // The editing scene is reset before each load, so the only stage director
        // present is the one just instantiated (the DebugCutsceneRig has no director).
        public static PlayableDirector FindActiveDirector()
        {
            return Object.FindAnyObjectByType<PlayableDirector>();
        }

        private static GameObject ResolvePrefabRoot(PlayableDirector director)
        {
            GameObject go = director.gameObject;

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (source != null)
                go = source;

            string path = AssetDatabase.GetAssetPath(go);
            if (string.IsNullOrEmpty(path))
                path = AssetDatabase.GetAssetPath(director);

            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }
}
