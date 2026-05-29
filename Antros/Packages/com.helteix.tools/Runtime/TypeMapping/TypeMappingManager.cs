using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Helteix.Tools.TypeMapping
{
    public static class TypeMappingManager
    {
        private static readonly Dictionary<string, Type> Cache = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void OnInitialize()
        {
            Cache.Clear();
            var collections = Resources.FindObjectsOfTypeAll<TypeMappingCollection>();
            foreach (TypeMappingCollection collection in collections)
            {
                foreach (TypeMapEntry typeMap in collection.Entries)
                {
                    if(Cache.ContainsKey(typeMap.guid))
                        continue;

                    Type type = Type.GetType(typeMap.typeInfos, false);
                    if(type != null)
                        Cache.Add(typeMap.guid, type);
                }
            }
        }


        internal static bool TryGetType(string guid, out Type type)
        {
            return TryGetTypeFromCache(guid, out type);
        }

        private static bool TryGetTypeFromCache(string guid, out Type type)
        {
#if UNITY_EDITOR
            if (Cache.TryGetValue(guid, out type))
                return true;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
            {
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript != null)
                {
                    type = monoScript.GetClass();
                    Cache.Add(guid, type);

                    return true;
                }
            }

            return false;

#else
            return Cache.TryGetValue(guid, out type);
#endif
        }

        public static bool IsValid(string guid) => TryGetType(guid, out _);
    }
}