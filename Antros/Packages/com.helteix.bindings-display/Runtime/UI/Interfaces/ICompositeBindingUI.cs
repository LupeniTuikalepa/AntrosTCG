using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Helteix.ControlDisplay.UI.Interfaces
{
    public interface ICompositeBindingUI
    {
        void Sync(IReadOnlyList<BindingDescription> compositeBindings, NameAndParameters composite);
    }
}