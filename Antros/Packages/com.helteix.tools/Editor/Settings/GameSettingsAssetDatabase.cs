using System;
using System.Collections.Generic;
using System.Linq;
using Helteix.Tools.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Helteix.Tools.Editor.Settings
{

    public class GameSettingsAssetDatabase : AssetPostprocessor
    {
        public static event Action<List<ScriptableObject>> OnSettingsCreatedOrModified;
        public static event Action<ScriptableObject> OnSettingsWillBeDeleted;


        public static readonly Dictionary<string, ScriptableObject> existingSettings;

        public static readonly IReadOnlyList<Type> CachedSettingsType;

        static GameSettingsAssetDatabase()
        {
            CachedSettingsType = TypeCache.GetTypesDerivedFrom(typeof(GameSettings<>))
                .Where(ctx => !ctx.IsAbstract)
                .ToList();

            existingSettings = new Dictionary<string, ScriptableObject>();
        }

        [InitializeOnLoadMethod]
        private static void Load()
        {
            existingSettings.Clear();

            foreach (var t in CachedSettingsType)
            {
                string typeName = t.FullName;
                var existings = AssetDatabase.FindAssets($"t:{typeName}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToDictionary(ctx => ctx, AssetDatabase.LoadAssetAtPath<ScriptableObject>);
                foreach (var e in existings)
                    existingSettings.Add(e.Key, e.Value);
            }
        }

        public static bool OnWillDeleteSettings(string path)
        {
            if (existingSettings.Remove(path, out var existing))
            {
                OnSettingsWillBeDeleted?.Invoke(existing);
                return true;
            }
            return false;
        }

        public static int GetSettingsTypeQuantity(Type type) => GetOfType(type).Count();

        public static IEnumerable<ScriptableObject> GetOfType(Type type) => existingSettings.Values
            .Where(ctx => type.IsAssignableFrom(ctx.GetType()));


        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {

            for (int i = 0; i < movedFromAssetPaths.Length; i++)
            {
                if (existingSettings.Remove(movedFromAssetPaths[i], out var scriptableObject))
                    existingSettings.Add(movedAssets[i], scriptableObject);
            }

            using (ListPool<ScriptableObject>.Get(out var list))
            {
                for (int i = 0; i < importedAssets.Length; i++)
                {
                    string path = importedAssets[i];

                    if (existingSettings.TryGetValue(path, out var scriptableObject))
                    {
                        list.Add(scriptableObject);
                    }
                    else
                    {
                        Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                        if (!CachedSettingsType.Contains(type))
                            continue;

                        scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                        existingSettings.Add(path, scriptableObject);
                        list.Add(scriptableObject);
                    }
                }

                if (list.Count > 0)
                {
                    OnSettingsCreatedOrModified?.Invoke(list);
                }
            }

        }

    }
}