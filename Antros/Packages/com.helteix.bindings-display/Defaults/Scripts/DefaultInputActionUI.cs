using Helteix.ControlDisplay.UI.Builders;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Helteix.BindingDisplay.Defaults
{
    public class DefaultInputActionUI : InputActionUIBuilder
    {
        [SerializeField]
        private TMP_Text actionName;


        protected override void Connect(InputAction action)
        {
            if(actionName)
                actionName.text = action.name;

            base.Connect(action);
        }
    }
}