using UnityEngine.InputSystem;

namespace Helteix.ControlDisplay.UI
{
    public interface IBindingDisplayEventListener
    {
        void OnBindingChanged(InputAction action);
        void OnEnableStateChanged(InputAction action);
    }
}