using System;
using System.Collections.Generic;
using Helteix.ControlDisplay.Data;
using Helteix.ControlDisplay.UI.Interfaces;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Pool;

namespace Helteix.ControlDisplay.UI.Builders
{
    public class InputActionUIBuilder : InputActionUI
    {
        [SerializeField]
        private Transform container;

        [SerializeField]
        private GameObject orPrefab;

        private void Awake()
        {
            container.ClearChildren();
        }

        protected override void Connect(InputAction action)
        {
            container.ClearChildren();

            using PooledObject<List<InputDevice>> pooledDevices = ListPool<InputDevice>.Get(out List<InputDevice> devices);
            ReadOnlyArray<InputBinding> bindings = action.bindings;

            GetDevicesForAction(action, devices);
            int spawned = 0;

            for (int i = 0; i < bindings.Count; i++)
            {
                InputBinding binding = bindings[i];

                if (string.IsNullOrEmpty(binding.effectiveInteractions))
                    binding.overrideInteractions = action.interactions;
                else if (!string.IsNullOrEmpty(action.interactions))
                    binding.overrideInteractions = binding.effectiveInteractions;

                if (binding.isComposite)
                {
                    CreateUIForCompositeBinding(i, bindings, devices);
                    continue;
                }

                if(binding.isPartOfComposite)
                    continue;

                if (TryGetBindingDescription(action, binding, devices, i, out BindingDescription description))
                {
                    if (spawned > 0 && orPrefab != null)
                        Instantiate(orPrefab, container);

                    spawned++;
                    description.SpawnUIWithActiveSettings(container);
                }
                else
                {
                    Debug.LogWarning($"Could not find binding for description: {description}");
                }
            }
        }

        protected override void Disconnect()
        {
            container.ClearChildren();
        }


        private void CreateUIForCompositeBinding(int i, ReadOnlyArray<InputBinding> bindings, List<InputDevice> devices)
        {
            NameAndParameters composite = NameAndParameters.Parse(bindings[i].effectivePath);
            if (!BindingDisplaySettings.Current.TryGetCompositeUIFor(composite.name, out GameObject compositeUI))
                return;

            GameObject instance = Instantiate(compositeUI, container);
            if (!instance.TryGetComponent(out ICompositeBindingUI compositeBindingUI))
                return;

            using (ListPool<BindingDescription>.Get(out var compositeBindingsDescriptions))
            {
                int currentBindingIndex = i + 1;
                while (currentBindingIndex < bindings.Count && bindings[currentBindingIndex].isPartOfComposite)
                {
                    if (TryGetBindingDescription(CurrentAction, bindings[i], devices, currentBindingIndex,
                            out BindingDescription bindingDescription))
                    {
                        compositeBindingsDescriptions.Add(bindingDescription);
                    }
                }

                compositeBindingUI.Sync(compositeBindingsDescriptions, composite);
            }
        }
        internal static void GetDevicesForAction(InputAction action, List<InputDevice> devices)
        {
            for (int i = 0; i < action.controls.Count; i++)
            {
                InputControl control = action.controls[i];
                if (devices.Contains(control.device))
                    continue;

                Debug.Log("Found device: " + control.device.displayName);

                devices.Add(control.device);
            }
        }

        internal static bool TryGetBindingDescription(InputAction action, InputBinding binding, List<InputDevice> devices, int bindingIndex, out BindingDescription description)
        {
            string displayString = binding.ToDisplayString(
                out string deviceLayoutName,
                out string controlPath,
                InputBinding.DisplayStringOptions.DontIncludeInteractions);

            InputDevice bindingDevice = null;
            InputControl control = null;

            foreach (InputDevice device in devices)
            {
                if (InputSystem.IsFirstLayoutBasedOnSecond(device.layout, deviceLayoutName) || device.layout == deviceLayoutName)
                {
                    control = device[controlPath];
                    displayString = control.displayName;
                    bindingDevice = device;

                    //Debug.Log($"Found display name : {effectiveBinding.effectivePath} => {control.displayName}");
                    break;
                }
            }

            if (bindingDevice == null)
            {
                description = default;
                return false;
            }

            NameAndParameters[] interactionsNameAndParams;
            if(string.IsNullOrEmpty(binding.effectiveInteractions))
                interactionsNameAndParams = Array.Empty<NameAndParameters>();
            else
            {
                string[] interactions = binding.effectiveInteractions.Split(';');
                interactionsNameAndParams = new NameAndParameters[interactions.Length];
                for (int i = 0; i < interactions.Length; i++)
                    interactionsNameAndParams[i] = NameAndParameters.Parse(interactions[i]);
            }

            description = new BindingDescription()
            {
                effectiveBinding = binding,
                device = bindingDevice,
                action = action,
                control = control,

                bindingIndex = bindingIndex,
                displayString = displayString,
                deviceLayout = deviceLayoutName,

                interactions = interactionsNameAndParams,
                controlPath = controlPath,
            };

            return true;
        }
    }
}