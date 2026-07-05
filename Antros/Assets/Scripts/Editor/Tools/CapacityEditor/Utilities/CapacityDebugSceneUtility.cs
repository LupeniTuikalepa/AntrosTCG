using ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Opens the shared capacity-editing scene, creating it on first use. The scene
    /// is a scratch workspace for authoring timelines — never referenced by any
    /// CapacityData asset. Its path lives in CapacityEditorSettings (versioned).
    /// </summary>
    public static class CapacityDebugSceneUtility
    {
        public static bool OpenOrCreate(out string statusMessage)
        {
            CapacityEditorSettings settings = CapacityEditorSettings.GetOrCreate();
            string path = settings.editingScenePath;

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
                return Create(settings, out statusMessage);

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                statusMessage = "Cancelled (unsaved changes in the current scene).";
                return false;
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            EnsureRig();
            statusMessage = $"Opened editing scene '{path}'.";
            return true;
        }

        private static bool Create(CapacityEditorSettings settings, out string statusMessage)
        {
            string suggested = string.IsNullOrEmpty(settings.editingScenePath)
                ? "CapacitiesEdition"
                : System.IO.Path.GetFileNameWithoutExtension(settings.editingScenePath);

            string chosen = EditorUtility.SaveFilePanelInProject(
                "Create Capacity Editing Scene",
                suggested,
                "unity",
                "Choose where to store the scratch scene used to author capacity timelines.",
                "Assets/Scenes/Editor");

            if (string.IsNullOrEmpty(chosen))
            {
                statusMessage = "Scene creation cancelled.";
                return false;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                statusMessage = "Cancelled (unsaved changes in the current scene).";
                return false;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject rigObject = new("DebugCutsceneRig");
            rigObject.AddComponent<DebugCutsceneRig>();

            EditorSceneManager.SaveScene(scene, chosen);

            settings.editingScenePath = chosen;
            settings.Save();

            statusMessage = $"Created and opened new editing scene at '{chosen}'.";
            return true;
        }

        // Guarantees the open editing scene has a rig for edit-mode binding.
        private static void EnsureRig()
        {
            if (Object.FindFirstObjectByType<DebugCutsceneRig>() == null)
            {
                GameObject rigObject = new("DebugCutsceneRig");
                rigObject.AddComponent<DebugCutsceneRig>();
                EditorSceneManager.MarkAllScenesDirty();
            }
        }
    }
}