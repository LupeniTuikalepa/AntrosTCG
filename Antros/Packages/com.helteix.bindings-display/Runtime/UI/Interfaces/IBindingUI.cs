using Helteix.ControlDisplay.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Helteix.ControlDisplay.UI.Interfaces
{
    public interface IBindingUI
    {
        void Sync(BindingDescription binding, BindingIcons icons);
    }
}