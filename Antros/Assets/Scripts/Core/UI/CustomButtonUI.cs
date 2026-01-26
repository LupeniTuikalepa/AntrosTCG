using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ATCG.Utilities;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace ATCG.UI
{
    public class CustomButtonUI : SelectableUIComponent
    {
        [Serializable]
        public struct ButtonShortcut
        {
            [field: SerializeField, InputControl]
            public string ControlPath { get; private set; }

            [field: SerializeField]
            public GameObject Root { get; private set; }
        }
        public UnityEvent<BaseEventData> OnClick => onClick;

        [SerializeField]
        private UnityEvent<BaseEventData> onClick;

        [SerializeField]
        private ButtonShortcut[] shortcuts;

        [SerializeField, Min(0)]
        private float holdDuration;
        [SerializeField]
        private Image holdFill;

        [ShowInInspector, HideInEditorMode]
        private InputAction shortcutsAction;

        private float holdTimer = 0;

        public bool Interactable
        {
            get => Selectable.interactable;
            set => Selectable.interactable = value;
        }
        protected override void Awake()
        {
            base.Awake();
            if(InputUser.all.Count > 0)
                OnUserChanged(InputUser.all[0]);
        }

        private void OnEnable()
        {
            shortcutsAction?.Enable();

            Selectable.OnPointerWasDown += OnPointerDown;
            Selectable.OnPointerWasUp += OnPointerUp;

            if (Manager)
            {
                Manager.OnUserChanged += OnUserChanged;
                Manager.OnNewUIInputActionAsset += Connect;

                Connect(null, Manager.ActiveUIInputActionAsset);
            }
        }

        private void OnDisable()
        {
            shortcutsAction?.Disable();

            Selectable.OnPointerWasDown -= OnPointerDown;
            Selectable.OnPointerWasUp -= OnPointerUp;

            if (Manager)
            {
                Manager.OnUserChanged -= OnUserChanged;
                Manager.OnNewUIInputActionAsset -= Connect;
            }
        }

        private void Connect(InputActionAsset oldAsset, InputActionAsset newAsset)
        {
            if (oldAsset)
            {
                oldAsset.RemoveAction(shortcutsAction.id.ToString());
                shortcutsAction.Dispose();
            }

            if (newAsset == null)
                return;

            bool wasEnabled = newAsset is { enabled: true };
            if(wasEnabled)
                newAsset.Disable();

            InputActionMap inputActionMap = newAsset.FindActionMap("UI");
            if (holdDuration == 0)
            {
                shortcutsAction = inputActionMap.AddAction($"Click {name} button");
                shortcutsAction.performed += OnShortcutPerformed;
            }
            else
            {
                shortcutsAction = inputActionMap.AddAction($"Click {name} button", interactions: $"hold(duration={holdDuration.ToString(CultureInfo.InvariantCulture)})");
                shortcutsAction.started += OnShortcutHoldBegin;
                shortcutsAction.performed += OnShortcutHoldPerformed;
                shortcutsAction.canceled += OnShortcutHoldCancel;
            }

            for (int i = 0; i < shortcuts.Length; i++)
                shortcutsAction.AddBinding(shortcuts[i].ControlPath);

            if(wasEnabled)
                newAsset.Enable();

            shortcutsAction?.Enable();
            if(Manager.CurrentUser.valid)
                OnUserChanged(Manager.CurrentUser);
        }

        private void OnDestroy()
        {
            shortcutsAction?.Dispose();
        }

        [Button]
        private void OnUserChanged() => OnUserChanged(Manager.CurrentUser);

        private void OnUserChanged(InputUser inputUser)
        {
            using (ListPool<InputDevice>.Get(out List<InputDevice> requiredDevices))
            {
                inputUser.GetRequiredDevices(requiredDevices);
                foreach (ButtonShortcut shortcut in shortcuts)
                {
                    bool isValid = false;

                    foreach (var device in requiredDevices)
                    {
                        InputControl control = device.allControls.FirstOrDefault(ctx => InputControlPath.Matches(shortcut.ControlPath, ctx));
                        isValid = control != null;
                        if (isValid)
                            break;

                    }

                    if (shortcut.Root != null)
                        shortcut.Root.SetActive(isValid);
                }
            }
        }

        private void OnShortcutPerformed(InputAction.CallbackContext ctx)
        {
            if(!Interactable)
                return;

            if(Selectable.IsInteractable())
                TriggerButton(new BaseEventData(EventSystem));
        }

        private void OnShortcutHoldPerformed(InputAction.CallbackContext ctx)
        {
            if(Interactable)
                TriggerButton(new BaseEventData(EventSystem));

            StopHoldAnimation();
        }


        private void OnShortcutHoldBegin(InputAction.CallbackContext obj)
        {
            if(!Interactable)
                return;
            StopHoldAnimation();
            BeginHoldAnimation();
        }

        private void OnShortcutHoldCancel(InputAction.CallbackContext obj)
        {
            if(!Interactable)
                return;
            
            StopHoldAnimation();
        }


        private void OnPointerDown(PointerEventData pointerEventData)
        {
            if(!Interactable)
                return;

            if (holdDuration > 0)
            {
                BeginHoldAnimation();
            }
            else
                TriggerButton(pointerEventData);
        }


        private void OnPointerUp(PointerEventData pointerEventData)
        {
            if(!Interactable)
                return;
            if (holdDuration > 0)
            {
                if(holdTimer > holdDuration)
                    TriggerButton(pointerEventData);
            }
            else
            {
                TriggerButton(pointerEventData);
            }

            StopHoldAnimation();
        }
        private void TriggerButton(BaseEventData eventData)
        {
            if(!Interactable)
                return;
            onClick?.Invoke(eventData);
        }
        private void BeginHoldAnimation()
        {
            holdFill.gameObject.SetActive(true);
            Tween.UIFillAmount(holdFill, 1, duration: holdDuration, Ease.OutQuad);
        }

        private void StopHoldAnimation()
        {
            Tween.StopAll(holdFill);
            holdTimer = 0;
            holdFill.gameObject.SetActive(false);
            holdFill.fillAmount = 0;
        }
    }
}