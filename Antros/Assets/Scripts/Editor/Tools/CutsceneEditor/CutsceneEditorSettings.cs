using System.IO;
using ATCG.Editor.Tools.CapacityEditor; // legacy CapacityEditorSettings, read once for migration
using UnityEditor;
using UnityEngine;

namespace ATCG.Editor.Tools.CutsceneEditor
{
    /// <summary>
    /// The single source of truth for the editor templates every cutscene kind relies on — the
    /// director prefab template (cloned as a variant when a stage is scaffolded), the test environment
    /// (instantiated inside the authoring stage), and the UMotion template (copied per capacity). Both
    /// the generic cutscene tooling and the capacity tooling read this one asset; the Cutscenes →
    /// Settings tab edits it.
    ///
    /// On first access it migrates the values from the legacy CapacityEditorSettings asset, so an
    /// existing project keeps its assigned templates with nothing to re-drag.
    /// </summary>
    public sealed class CutsceneEditorSettings : ScriptableObject
    {
        private const string SettingsPath = "Assets/Editor Default Resources/CutsceneEditorSettings.asset";
        private const string LegacyPath = "Assets/Editor Default Resources/CapacityEditorSettings.asset";

        [Tooltip("Prefab template new cutscene stages are created from (as prefab variants).")]
        public GameObject directorTemplate;

        [Tooltip("Test environment instantiated inside the cutscene edit stage: hero prefab, camera " +
                 "with CinemachineBrain, and the DebugCutsceneRig. The reusable 'scene' cutscenes are " +
                 "authored against — scenery, never saved.")]
        public GameObject testEnvironmentPrefab;

        [Tooltip("UMotion template copied into a capacity's folder as {Name}Motion when it's created, " +
                 "so every capacity starts from the same UMotion setup.")]
        public Object umotionTemplate;

        private static CutsceneEditorSettings cached;

        public static CutsceneEditorSettings GetOrCreate()
        {
            if (cached != null)
                return cached;

            cached = AssetDatabase.LoadAssetAtPath<CutsceneEditorSettings>(SettingsPath);
            if (cached != null)
                return cached;

            cached = CreateInstance<CutsceneEditorSettings>();
            bool migrated = MigrateFromLegacy(cached);

            string dir = Path.GetDirectoryName(SettingsPath);
            if (!AssetDatabase.IsValidFolder(dir))
                Directory.CreateDirectory(dir);

            AssetDatabase.CreateAsset(cached, SettingsPath);
            AssetDatabase.SaveAssets();

            // Only once the new asset is safely on disk: drop the legacy one so no duplicate lingers.
            if (migrated)
            {
                AssetDatabase.DeleteAsset(LegacyPath);
                Debug.Log("[CutsceneEditor] Migrated editor template settings to CutsceneEditorSettings " +
                          "and removed the legacy CapacityEditorSettings asset.");
            }

            return cached;
        }

        // Copies the template references from the legacy capacity-editor settings the first time the
        // new asset is created, so existing projects don't lose their setup. Returns whether a legacy
        // asset was found (and therefore should be removed afterwards).
        private static bool MigrateFromLegacy(CutsceneEditorSettings target)
        {
            CapacityEditorSettings legacy = AssetDatabase.LoadAssetAtPath<CapacityEditorSettings>(LegacyPath);
            if (legacy == null)
                return false;

            target.directorTemplate = legacy.directorTemplate;
            target.testEnvironmentPrefab = legacy.testEnvironmentPrefab;
            target.umotionTemplate = legacy.umotionTemplate;
            return true;
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}
