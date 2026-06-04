using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Pool;

namespace Helteix.ControlDisplay.Debugging
{
    [ExecuteInEditMode]
    public class LogInputAction : MonoBehaviour
    {
        [SerializeField]
        private InputAction action;

        [SerializeField, InputControl]
        private string controlPath;

        private void Update()
        {
            using (ListPool<InputDevice>.Get(out var devices))
            {
                for (int i = 0; i < action.controls.Count; i++)
                {
                    InputControl control = action.controls[i];
                    if(devices.Contains(control.device))
                        continue;

                    devices.Add(control.device);
                }

                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding binding = action.bindings[i];
                    if (binding.isComposite)
                        continue;

                    if (string.IsNullOrEmpty(binding.effectiveInteractions))
                        binding.overrideInteractions = action.interactions;
                    else if (!string.IsNullOrEmpty(action.interactions))
                        binding.overrideInteractions = binding.effectiveInteractions;

                    string displayString = binding.ToDisplayString(out var deviceLayoutName, out string controlPath,
                        InputBinding.DisplayStringOptions.DontIncludeInteractions);

                    InputControl control = null;
                    foreach (var device in devices)
                    {
                        if (InputSystem.IsFirstLayoutBasedOnSecond(device.layout, deviceLayoutName))
                        {
                            control = device[controlPath];
                            //Debug.Log($"Found display name : {effectiveBinding.effectivePath} => {control.displayName}");
                            displayString = control.displayName;
                            deviceLayoutName = device.layout;
                            break;
                        }
                    }

                    Debug.Log($"Action : {displayString} " +
                              $"- Device : {deviceLayoutName} " +
                              $"- Control : {controlPath} " +
                              $"- Interaction : {binding.effectiveInteractions}");
                }
            }
        }
    }
}