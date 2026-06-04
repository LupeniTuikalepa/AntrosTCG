using Helteix.ControlDisplay.UI.Interfaces;
using Helteix.Tools.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Helteix.ControlDisplay.Data
{
    [CreateAssetMenu(menuName = "Helteix/Binding Display/Settings")]
    public class BindingDisplaySettings : GameSettings<BindingDisplaySettings>
    {
        [field: SerializeField]
        public bool CopyListenersListBeforeCallbacks { get; private set; } = false;

        [field: SerializeField, Range(1, 5)]
        public int SpriteVariantsCount { get; private set; } = 3;

        [field: SerializeField]
        public LayoutEntry[] Devices { get; private set; }

        [field: SerializeField]
        public CompositeEntry[] Composites { get; private set; }

        public GameObject SpawnUIFor(in BindingDescription description, Transform container)
        {
            if (!TryGetPrefabFor(description, out LayoutEntry layout, out BindingEntry entry))
                return null;

            var prefab = entry.OverridePrefab == null ? layout.BindingPrefab : entry.OverridePrefab;
            if(prefab == null)
            {
                Debug.LogError($"Found entry for path {description.controlPath} but no prefab was assigned in the device layout group or in override. \n" +
                               $"Did you forget to assign one of them?");
                return null;
            }


            GameObject instance = Instantiate(prefab, container);
            if (instance.TryGetComponent(out IBindingUI bindingUI))
                bindingUI.Sync(description,  entry.Icons);

            return instance;
        }

        public bool TryGetPrefabFor(BindingDescription description, out LayoutEntry layoutEntry, out BindingEntry entry)
        {
            string deviceLayout = description.device.layout;
            for (int i = 0; i < Devices.Length; i++)
            {
                layoutEntry= Devices[i];
                if (IsLayoutValid(deviceLayout, layoutEntry))
                {
                    if (layoutEntry.TryGetEntryFor(description, out entry))
                        return true;
                }
            }

            Debug.LogWarning($"No entry was matching {description.controlPath}");
            entry = default;
            layoutEntry = default;
            return false;
        }

        private static bool IsLayoutValid(string deviceLayout, LayoutEntry layout)
        {
            return deviceLayout == layout.Layout || InputSystem.IsFirstLayoutBasedOnSecond(deviceLayout, layout.Layout);
        }

        public bool TryGetCompositeUIFor(string compositeName, out GameObject prefab)
        {
            for (int i = 0; i < Composites.Length; i++)
            {
                CompositeEntry entry = Composites[i];
                if (entry.CompositeName != compositeName)
                    continue;

                prefab = entry.Prefab;
                return true;
            }

            prefab = null;
            return false;
        }
    }
}