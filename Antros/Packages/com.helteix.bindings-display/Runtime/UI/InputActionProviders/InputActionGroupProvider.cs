using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Helteix.ControlDisplay.UI.InputActionProviders
{
    public abstract class InputActionGroupProvider : MonoBehaviour
    {
        public abstract IEnumerable<InputAction> GetActions();
    }
}