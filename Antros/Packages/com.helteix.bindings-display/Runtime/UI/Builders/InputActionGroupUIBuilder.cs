using System;
using System.Collections.Generic;
using Helteix.ControlDisplay.UI.InputActionProviders;
using Helteix.ControlDisplay.UI.Interfaces;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Helteix.ControlDisplay.UI.Builders
{
    public class InputActionGroupUIBuilder : MonoBehaviour, IBindingDisplayEventListener
    {
        [SerializeField]
        private InputActionUI actionUIPrefab;

        [SerializeField]
        private Transform container;
        [SerializeField]
        private InputActionGroupProvider groupProvider;

        [SerializeField]
        private bool buildOnStart;

        [SerializeField]
        private bool showActiveActionsOnly;

        private Dictionary<InputAction, InputActionUI> inputActionUis;

        private void Awake()
        {
            inputActionUis = new();
            Clear();
        }

        private void Start()
        {
            if(buildOnStart)
                Build();
        }

        public void Build()
        {
            Clear();

            foreach (InputAction action in groupProvider.GetActions())
            {
                if (!inputActionUis.TryAdd(action, null))
                    continue;
                InputActionUI ui = Instantiate(actionUIPrefab, container);
                inputActionUis[action] = ui;

                ui.ConnectUI(action);
                this.Register(action);

                if(showActiveActionsOnly)
                    ui.gameObject.SetActive(action.enabled);
            }
        }

        public void Clear()
        {
            foreach ((InputAction inputAction, InputActionUI inputActionUI) in inputActionUis)
            {
                this.Unregister(inputAction);
                inputActionUI.DisconnectUI();

                Destroy(inputActionUI);
            }


            container.ClearChildren();
        }

        void IBindingDisplayEventListener.OnBindingChanged(InputAction action)
        {
            if (inputActionUis.TryGetValue(action, out InputActionUI ui))
                ui.Reconnect();
        }

        void IBindingDisplayEventListener.OnEnableStateChanged(InputAction action)
        {
            if (showActiveActionsOnly && inputActionUis.TryGetValue(action, out var ui))
                ui.gameObject.SetActive(action.enabled);
        }
    }
}