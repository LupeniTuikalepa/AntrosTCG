using UnityEditor;
using UnityEngine;

namespace ATCG.Editor.Tools.Characters
{
    /// <summary>
    /// Persistent settings for the Characters tool. Notably the FlatKit "base material": on export, a
    /// COPY of it replaces the material Synty generates for the character, keeping the character's own
    /// albedo in FlatKit's base map — so exported characters come out already styled. Also holds the
    /// folder the Explore sub-tab scans for saved <c>.sk</c> characters.
    /// </summary>
    public sealed class CharacterToolSettings : ScriptableObject
    {
        private const string Folder = "Assets/Editor Default Resources";
        private const string SettingsPath = Folder + "/CharacterToolSettings.asset";

        [SerializeField]
        [Tooltip("FlatKit material whose copy replaces the exported character material (its albedo goes into the base map).")]
        private Material baseMaterial;

        [SerializeField]
        [Tooltip("Folder scanned by the Explore tab for .sk characters. Defaults to Assets/Project/Characters.")]
        private DefaultAsset exploreFolder;

        public Material BaseMaterial
        {
            get => baseMaterial;
            set { baseMaterial = value; Save(); }
        }

        public DefaultAsset ExploreFolder
        {
            get => exploreFolder;
            set { exploreFolder = value; Save(); }
        }

        private static CharacterToolSettings cached;

        public static CharacterToolSettings GetOrCreate()
        {
            if (cached != null)
                return cached;

            cached = AssetDatabase.LoadAssetAtPath<CharacterToolSettings>(SettingsPath);
            if (cached != null)
                return cached;

            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets", "Editor Default Resources");

            cached = CreateInstance<CharacterToolSettings>();
            AssetDatabase.CreateAsset(cached, SettingsPath);
            AssetDatabase.SaveAssets();
            return cached;
        }

        private void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
