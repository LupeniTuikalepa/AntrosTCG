using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Helteix.Tools.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Helteix.Tools.Editor.Settings
{
    [FilePath("ProjectSettings/LTX/GameSettingsEditorPrefs.Asset", FilePathAttribute.Location.ProjectFolder)]
    public class GameSettingsEditorPrefs : ScriptableSingleton<GameSettingsEditorPrefs>
    {
        public event Action<ScriptableObject, Type> OnNewActiveSettings;

        [field: SerializeField]
        public string DefaultSettingsPath { get; internal set; } = "Settings";

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(DefaultSettingsPath))
                DefaultSettingsPath = "Settings";

            CreateMissingSettings();
        }


        public void Save() => instance.Save(true);

        public ScriptableObject CreateNewSettings(Type t)
        {
            ScriptableObject newSettingsInstance = CreateInstance(t);
            string defaultPath = Path.Combine("Assets", DefaultSettingsPath);
            string folderPath = AssetDatabase.IsValidFolder(defaultPath) ? defaultPath : "Assets";

            string assetPath = Path.Combine(folderPath, t.Name + ".asset");

            AssetDatabase.CreateAsset(newSettingsInstance, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            return newSettingsInstance;
        }


        private void CreateMissingSettings()
        {
            foreach (var t in GameSettingsAssetDatabase.CachedSettingsType)
            {
                ScriptableObject[] scriptableObjects = GameSettingsAssetDatabase
                    .GetOfType(t)
                    .ToArray();
                if (scriptableObjects.Length != 0)
                    continue;

                var att = t.GetCustomAttribute<AutoGenerateGameSettingsAttribute>();
                if(att == null)
                    continue;

                var newSettings = CreateNewSettings(t);
                SetAsDefault(newSettings, t);
            }

            using (ListPool<Object>.Get(out var list))
            {
                Object[] preloaded = PlayerSettings.GetPreloadedAssets();
                list.AddRange(preloaded);
                foreach (var t in GameSettingsAssetDatabase.CachedSettingsType)
                {
                    ScriptableObject[] settings = GameSettingsAssetDatabase
                        .GetOfType(t)
                        .ToArray();

                    if (settings.Length == 0)
                        continue;

                    if (preloaded.Any(ctx => ctx != null && t.IsAssignableFrom(ctx.GetType())))
                        continue;

                    list.Add(settings[0]);
                }

                list.RemoveAll(ctx => ctx == null);
                PlayerSettings.SetPreloadedAssets(list.ToArray());
            }

            SerializedObject serializedObject = new SerializedObject(this);
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
        }

        public void SetAsDefault(ScriptableObject scriptableObject, Type type)
        {
            Type soType = scriptableObject.GetType();
            if (!type.IsAssignableFrom(soType))
                return;

            Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();
            for (int i = 0; i < preloadedAssets.Length; i++)
            {
                Object asset = preloadedAssets[i];
                if(asset == null)
                    continue;

                if (type.IsAssignableFrom(asset.GetType()))
                {
                    preloadedAssets[i] = scriptableObject;
                    PlayerSettings.SetPreloadedAssets(preloadedAssets);

                    OnNewActiveSettings?.Invoke(scriptableObject, type);
                    return;
                }
            }

            var list = new List<Object>(preloadedAssets);
            list.Add(scriptableObject);
            PlayerSettings.SetPreloadedAssets(list.ToArray());

            OnNewActiveSettings?.Invoke(scriptableObject, type);
        }


        public bool IsDefault(ScriptableObject scriptableObject)
        {
            if(scriptableObject == null)
                return false;

            Object[] preloadedAssets = PlayerSettings.GetPreloadedAssets();
            for (int i = 0; i < preloadedAssets.Length; i++)
            {
                if(scriptableObject == preloadedAssets[i])
                    return true;
            }

            return false;
        }

    }
}