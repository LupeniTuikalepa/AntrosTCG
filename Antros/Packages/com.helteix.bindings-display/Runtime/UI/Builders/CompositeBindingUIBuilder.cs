using System.Collections.Generic;
using Helteix.ControlDisplay.Data;
using Helteix.ControlDisplay.UI.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace Helteix.ControlDisplay.UI.Builders
{
    public class CompositeBindingUIBuilder : MonoBehaviour, ICompositeBindingUI
    {
        [SerializeField]
        private Transform container;

        public void Sync(IReadOnlyList<BindingDescription> compositeBindings, NameAndParameters composite)
        {
            foreach (var description in compositeBindings)
                description.SpawnUIWithActiveSettings(container);
        }
    }
}