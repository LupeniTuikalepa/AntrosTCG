using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Helteix.ControlDisplay.UI.InputActionProviders
{
    public class PlayerInputActionGroupProvider : InputActionGroupProvider
    {
        [SerializeField]
        private PlayerInput playerInput;
        [SerializeField]
        private bool useFilters;
        [SerializeField]
        private string[] paths;

        public override IEnumerable<InputAction> GetActions()
        {
            if (useFilters)
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    InputAction action = playerInput.actions.FindAction(paths[i], false);
                    if (action != null)
                        yield return action;
                }
            }
            else
            {
                foreach (InputAction action in playerInput.actions)
                    yield return action;
            }
        }
    }
}