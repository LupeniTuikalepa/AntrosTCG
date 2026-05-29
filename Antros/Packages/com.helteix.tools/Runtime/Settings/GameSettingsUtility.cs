using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Helteix.Tools.Settings
{
    public abstract partial class GameSettings<T>
    {
        private static T GetCurrent()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var preloaded = PlayerSettings.GetPreloadedAssets();
                for (int i = 0; i < preloaded.Length; i++)
                {
                    if(preloaded[i] is T asset)
                        return asset;
                }
            }
#endif

            if (activeSettingsIndex != -1)
                return LoadedSettings[activeSettingsIndex];

            T[] loaded = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < loaded.Length; i++)
            {
                if(!LoadedSettings.Contains(loaded[i]))
                    LoadedSettings.Add(loaded[i]);
            }

            if (LoadedSettings.Count == 0)
            {
                Debug.Log(
                    $"No settings assets were found for {typeof(T).Name}. Creating a new one temporarily with default values.");

                T gameSettings = CreateInstance<T>();
                LoadedSettings.Add(gameSettings);
            }

            activeSettingsIndex = 0;
            return LoadedSettings[activeSettingsIndex];
        }
    }
}