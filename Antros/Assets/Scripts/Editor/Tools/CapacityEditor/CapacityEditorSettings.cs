using UnityEditor;
using UnityEngine;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Versioned settings for the Capacity Editor tool. Stored as an asset (not
    /// EditorPrefs) so it survives recompiles, is a real asset reference that follows
    /// renames, and is shared across the team via VCS.
    /// </summary>
    public sealed class CapacityEditorSettings : ScriptableObject
    {
        private const string SettingsPath = "Assets/Editor Default Resources/CapacityEditorSettings.asset";

        [Tooltip("Prefab template new cutscene stages are created from (as prefab variants).")]
        public GameObject directorTemplate;

        [Tooltip("Test environment instantiated inside the cutscene edit stage: hero prefab " +
                 "(for animations), camera with CinemachineBrain, and the DebugCutsceneRig for " +
                 "edit-mode binding. Acts as the reusable 'scene' the cutscene is authored against.")]
        public GameObject testEnvironmentPrefab;

        private static CapacityEditorSettings cached;

        public static CapacityEditorSettings GetOrCreate()
        {
            if (cached != null)
                return cached;

            cached = AssetDatabase.LoadAssetAtPath<CapacityEditorSettings>(SettingsPath);
            if (cached != null)
                return cached;

            cached = CreateInstance<CapacityEditorSettings>();

            string dir = System.IO.Path.GetDirectoryName(SettingsPath);
            if (!AssetDatabase.IsValidFolder(dir))
                System.IO.Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(cached, SettingsPath);
            AssetDatabase.SaveAssets();
            return cached;
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}