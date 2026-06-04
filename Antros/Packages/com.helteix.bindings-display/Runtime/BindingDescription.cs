using Helteix.ControlDisplay.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Helteix.ControlDisplay
{
    public struct BindingDescription
    {
        public InputBinding effectiveBinding;
        public InputDevice device;
        public InputAction action;
        public InputControl control;

        public NameAndParameters[] interactions;

        public string displayString;
        public string deviceLayout;
        public string controlPath;

        public int bindingIndex;

        public GameObject SpawnUIWithActiveSettings(Transform container) => BindingDisplaySettings.Current.SpawnUIFor(this, container);
    }
}