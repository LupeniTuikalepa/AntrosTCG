using System.Collections.Generic;
using System.Reflection;
using ATCG.Capacities;
using UnityEditor;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Discovers all CapacityData assets in the project and groups them by their
    /// Element value, read by reflection so no hard dependency on the enum type is
    /// needed and new Element values are picked up automatically.
    /// </summary>
    public static class CapacityCatalog
    {
        public readonly struct Entry
        {
            public readonly string ElementName;
            public readonly CapacityData Capacity;

            public Entry(string elementName, CapacityData capacity)
            {
                ElementName = elementName;
                Capacity = capacity;
            }
        }

        // Ordered list grouped by element (element name -> capacities under it).
        public static List<KeyValuePair<string, List<CapacityData>>> GroupedByElement()
        {
            Dictionary<string, List<CapacityData>> byElement = new();

            string[] guids = AssetDatabase.FindAssets($"t:{nameof(CapacityData)}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CapacityData capacity = AssetDatabase.LoadAssetAtPath<CapacityData>(path);
                if (capacity == null)
                    continue;

                string element = ReadElement(capacity);
                if (!byElement.TryGetValue(element, out List<CapacityData> list))
                {
                    list = new List<CapacityData>();
                    byElement[element] = list;
                }
                list.Add(capacity);
            }

            List<KeyValuePair<string, List<CapacityData>>> result = new(byElement);
            result.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            foreach (var kv in result)
                kv.Value.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

            return result;
        }

        // Reads the 'Element' property/field by reflection; falls back to "Unsorted".
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