using System;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable StaticMemberInGenericType

namespace Helteix.Tools.Settings
{
    public abstract partial class GameSettings<T> : ScriptableObject where T : GameSettings<T>
    {
        public static T Current => GetCurrent();

        public static IEnumerable<T> All => LoadedSettings;

        private static readonly List<T> LoadedSettings;

        private static int activeSettingsIndex;

        private static void ValidateIndex()
        {
            bool isEmpty = LoadedSettings.Count == 0;
            if (isEmpty)
            {
                activeSettingsIndex = -1;
                return;
            }

            if(activeSettingsIndex == -1)
                activeSettingsIndex = 0;

            if(activeSettingsIndex >= LoadedSettings.Count)
                activeSettingsIndex = LoadedSettings.Count - 1;
        }

        static GameSettings()
        {
            LoadedSettings = new List<T>();
            activeSettingsIndex = -1;
        }

        private void OnEnable()
        {
            Register();
            OnLoaded();
        }

        private void OnDisable()
        {
            Unregister();
            OnUnloaded();
        }


        protected virtual void OnLoaded() { }
        protected virtual void OnUnloaded() { }

        public void Register()
        {
            if (this is not T t)
                return;

            if (LoadedSettings.Contains(t))
                return;

            LoadedSettings.Add(t);
            ValidateIndex();
        }

        public void Unregister()
        {
            if (this is not T t)
                return;

            LoadedSettings.Remove(t);
            ValidateIndex();
        }

        public void SetActive()
        {
            if(this is T t)
            {
                int idx = LoadedSettings.IndexOf(t);
                if(idx != -1)
                    activeSettingsIndex = idx;
            }
        }

    }
}