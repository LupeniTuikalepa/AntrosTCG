using System.Reflection;
using ATCG.Capacities;
using UnityEditor;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Resolves and creates the canonical folder layout for a capacity's cutscene stage
    /// assets: Assets/Project/Cutscenes/Capacities/{Element}/{CapacityName}/ — with the prefab
    /// variant and its timeline living side by side, grouped under the shared Cutscenes root by
    /// kind. Element is read by reflection so no hard dependency on the enum type; capacity name is
    /// the CapacityData asset name.
    /// </summary>
    public static class CapacityAssetLayout
    {
        private const string CapacitiesRoot = "Assets/Project/Cutscenes/Capacities";

        public static string CapacityName(CapacityData capacity) => capacity.name;

        public static string DirectorPrefabPath(CapacityData capacity)
            => $"{CapacityFolder(capacity)}/{CapacityName(capacity)}Director.prefab";

        public static string TimelinePath(CapacityData capacity)
            => $"{CapacityFolder(capacity)}/{CapacityName(capacity)}Timeline.playable";

        // Ensures Assets/Project/Capacities/{Element}/{CapacityName} exists, creating
        // each missing segment, and returns the leaf folder path.
        public static string EnsureCapacityFolder(CapacityData capacity)
        {
            string element = ReadElement(capacity);
            string elementFolder = EnsureSubfolder(CapacitiesRoot, element);
            return EnsureSubfolder(elementFolder, CapacityName(capacity));
        }

        public static string CapacityFolder(CapacityData capacity)
            => $"{CapacitiesRoot}/{ReadElement(capacity)}/{CapacityName(capacity)}";

        // Creates 'parent/child' if absent, creating 'parent' chain up from Assets first.
        private static string EnsureSubfolder(string parent, string child)
        {
            EnsureFolderChain(parent);
            string full = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, child);
            return full;
        }

        // Walks a path from "Assets" downward, creating each missing folder.
        private static void EnsureFolderChain(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string[] parts = path.Split('/');
            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string ReadElement(CapacityData capacity)
        {
            System.Type type = capacity.GetType();

            PropertyInfo prop = type.GetProperty("Element",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            object value = prop != null ? prop.GetValue(capacity) : null;

            if (value == null)
            {
                FieldInfo field = type.GetField("Element",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                value = field?.GetValue(capacity);
            }

            return value != null ? value.ToString() : "Unsorted";
        }
    }
}