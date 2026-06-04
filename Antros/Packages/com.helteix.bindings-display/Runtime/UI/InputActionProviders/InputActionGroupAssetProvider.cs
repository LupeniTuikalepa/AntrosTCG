using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Helteix.ControlDisplay.UI.InputActionProviders
{
    public class InputActionGroupAssetProvider : InputActionGroupProvider
    {
        [SerializeField]
        private InputActionAsset actionAsset;

        [SerializeField]
        private string[] paths;

        public override IEnumerable<InputAction> GetActions()
        {

            for (int i = 0; i < paths.Length; i++)
            {
                InputAction action = actionAsset.FindAction(paths[i], false);
                if(action != null)
                    yield return action;
            }
        }
    }
}