using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Capacities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Instantiates a capacity's CutsceneDirector prefab into the currently-open
    /// editing scene, replacing whatever was loaded before. The director inside the
    /// prefab already carries the authored TimelineAsset (playableAsset), so the
    /// scene instance inherits it — nothing to push here.
    /// </summary>
    public static class CapacityStageInstantiator
    {
        public static CapacityCutscene LoadStage(CapacityData capacity)
        {
            if (capacity == null || capacity.CutsceneDirector == null)
                return null;

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(capacity));

            CapacityStageAnchor existing = Object.FindFirstObjectByType<CapacityStageAnchor>();
            if (existing != null)
            {
                if (existing.CapacityGuid == guid)
                    return existing.GetComponentInChildren<CapacityCutscene>();

                Object.DestroyImmediate(existing.gameObject);
            }

            // CutsceneDirector is a component living in the prefab asset; its
            // gameObject IS the prefab GO, so InstantiatePrefab takes it directly.
            // The prefab root may be an ancestor if the director sits on a child.
            GameObject prefabGo = capacity.CutsceneDirector.transform.root.gameObject;
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabGo);

            CapacityStageAnchor anchor = instance.AddComponent<CapacityStageAnchor>();
            anchor.CapacityGuid = guid;

            EditorSceneManager.MarkSceneDirty(instance.scene);

            return instance.GetComponentInChildren<CapacityCutscene>();
        }

        public static PlayableDirector FindActiveDirector()
        {
            CapacityStageAnchor anchor = Object.FindFirstObjectByType<CapacityStageAnchor>();
            return anchor != null ? anchor.GetComponentInChildren<PlayableDirector>() : null;
        }
    }
}