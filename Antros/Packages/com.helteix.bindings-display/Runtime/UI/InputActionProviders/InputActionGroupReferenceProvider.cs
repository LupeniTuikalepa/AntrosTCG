using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Helteix.ControlDisplay.UI.InputActionProviders
{
    public class InputActionGroupReferenceProvider : InputActionGroupProvider
    {
        [SerializeField]
        private InputActionReference[] references;

        public override IEnumerable<InputAction> GetActions()
        {
            for (int i = 0; i < references.Length; i++)
            {
                if (references[i] is { action: not null })
                    yield return references[i].action;
            }
        }
    }
}