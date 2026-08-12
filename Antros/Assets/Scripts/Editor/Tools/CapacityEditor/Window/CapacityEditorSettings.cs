using UnityEditor;
using UnityEngine;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// LEGACY — superseded by <c>CutsceneEditorSettings</c>, which is now the single source of truth
    /// for the editor templates. Kept only so the new settings can migrate this asset's values on
    /// first launch. Once every project has migrated, this class and its asset
    /// (Assets/Editor Default Resources/CapacityEditorSettings.asset) can be deleted.
    /// </summary>
    public sealed class CapacityEditorSettings : ScriptableObject
    {
        private const string SettingsPath = "Assets/Editor Default Resources/CapacityEditorSettings.asset";

        [Tooltip("Prefab template new cutscene stages are created from (as prefab variants).")]
        public GameObject directorTemplate;

        [Tooltip("UMotion template asset. When a new capacity is created, this is copied into the " +
                 "capacity's folder as {Name}Motion so each capacity starts from the same UMotion setup.")]
        public Object umotionTemplate;

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