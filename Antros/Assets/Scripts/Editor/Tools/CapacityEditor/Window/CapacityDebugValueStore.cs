using System;
using ATCG.Capacities.Properties;
using UnityEditor;
using UnityEngine;

namespace ATCG.Editor.Tools.CapacityEditor
{
    /// <summary>
    /// Editor-only store for a capacity's debug/preview property values. Kept OUT of the
    /// asset: persisted in EditorPrefs per {capacityGuid}:{propertyName}, so tweaked
    /// state survives switching capacities and recompiles, per machine. Handles the
    /// editable element types (float/int/bool/string/Vector3/HexCoordinates), single or
    /// array. Non-editable element types have no debug value.
    /// </summary>
    public static class CapacityDebugValueStore
    {
        private const string Prefix = "ATCG.CapacityEditor.DebugValue.";

        private static string Key(string capacityGuid, string propertyName)
            => $"{Prefix}{capacityGuid}:{propertyName}";

        public static bool TryGet(string capacityGuid, ICapacityPropertyDefinition def, out object value)
        {
            value = null;
            if (!IsEditable(def))
                return false;

            string key = Key(capacityGuid, def.Name);
            if (!EditorPrefs.HasKey(key))
                return false;

            try
            {
                value = Deserialize(def, EditorPrefs.GetString(key));
                return value != null;
            }
            catch
            {
                return false;
            }
        }

        public static void Set(string capacityGuid, ICapacityPropertyDefinition def, object value)
        {
            if (!IsEditable(def))
                return;
            EditorPrefs.SetString(Key(capacityGuid, def.Name), Serialize(def, value));
        }

        public static bool IsEditable(ICapacityPropertyDefinition def)
            => def != null && IsEditableElement(def.ElementType);

        public static bool IsEditableElement(Type t)
        {
            return t == typeof(float) || t == typeof(int) || t == typeof(bool) || t == typeof(string)
                || t == typeof(Vector3) || t == typeof(ATCG.HexGrids.HexCoordinates);
        }

        // ---- (de)serialization ----------------------------------------------
        // JSON via typed wrappers; arrays wrap the element array directly.

        [Serializable] private class Box<T> { public T v; }

        private static string Serialize(ICapacityPropertyDefinition def, object value)
        {
            Type e = def.ElementType;
            if (def.IsArray)
            {
                if (e == typeof(float)) return JsonUtility.ToJson(new Box<float[]> { v = (float[])value });
                if (e == typeof(int)) return JsonUtility.ToJson(new Box<int[]> { v = (int[])value });
                if (e == typeof(bool)) return JsonUtility.ToJson(new Box<bool[]> { v = (bool[])value });
                if (e == typeof(string)) return JsonUtility.ToJson(new Box<string[]> { v = (string[])value });
                if (e == typeof(Vector3)) return JsonUtility.ToJson(new Box<Vector3[]> { v = (Vector3[])value });
                if (e == typeof(ATCG.HexGrids.HexCoordinates)) return JsonUtility.ToJson(new Box<ATCG.HexGrids.HexCoordinates[]> { v = (ATCG.HexGrids.HexCoordinates[])value });
            }
            else
            {
                if (e == typeof(float)) return JsonUtility.ToJson(new Box<float> { v = (float)value });
                if (e == typeof(int)) return JsonUtility.ToJson(new Box<int> { v = (int)value });
                if (e == typeof(bool)) return JsonUtility.ToJson(new Box<bool> { v = (bool)value });
                if (e == typeof(string)) return JsonUtility.ToJson(new Box<string> { v = (string)value });
                if (e == typeof(Vector3)) return JsonUtility.ToJson(new Box<Vector3> { v = (Vector3)value });
                if (e == typeof(ATCG.HexGrids.HexCoordinates)) return JsonUtility.ToJson(new Box<ATCG.HexGrids.HexCoordinates> { v = (ATCG.HexGrids.HexCoordinates)value });
            }
            return string.Empty;
        }

        private static object Deserialize(ICapacityPropertyDefinition def, string raw)
        {
            Type e = def.ElementType;
            if (def.IsArray)
            {
                if (e == typeof(float)) return JsonUtility.FromJson<Box<float[]>>(raw).v;
                if (e == typeof(int)) return JsonUtility.FromJson<Box<int[]>>(raw).v;
                if (e == typeof(bool)) return JsonUtility.FromJson<Box<bool[]>>(raw).v;
                if (e == typeof(string)) return JsonUtility.FromJson<Box<string[]>>(raw).v;
                if (e == typeof(Vector3)) return JsonUtility.FromJson<Box<Vector3[]>>(raw).v;
                if (e == typeof(ATCG.HexGrids.HexCoordinates)) return JsonUtility.FromJson<Box<ATCG.HexGrids.HexCoordinates[]>>(raw).v;
            }
            else
            {
                if (e == typeof(float)) return JsonUtility.FromJson<Box<float>>(raw).v;
                if (e == typeof(int)) return JsonUtility.FromJson<Box<int>>(raw).v;
                if (e == typeof(bool)) return JsonUtility.FromJson<Box<bool>>(raw).v;
                if (e == typeof(string)) return JsonUtility.FromJson<Box<string>>(raw).v;
                if (e == typeof(Vector3)) return JsonUtility.FromJson<Box<Vector3>>(raw).v;
                if (e == typeof(ATCG.HexGrids.HexCoordinates)) return JsonUtility.FromJson<Box<ATCG.HexGrids.HexCoordinates>>(raw).v;
            }
            return null;
        }
    }
}