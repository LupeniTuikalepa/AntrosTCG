using System;
using Helteix.ControlDisplay.UI.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace ATCG.UI
{
    public class CustomButtonUI : SelectableUIComponent, IPointerClickHandler, ISubmitHandler
    {
        public UnityEvent<BaseEventData> OnClick => onClick;

        [SerializeField]
        private UnityEvent<BaseEventData> onClick;

        [SerializeField]
        private Image holdFill;

        [SerializeField]
        private CustomButtonActionProvider actionProvider;
        [SerializeField]
        private InputActionUI shortcutUI;


        private InputAction action;

        public bool Interactable
        {
            get => Selectable.interactable && action is null or { enabled: true };
            set => Selectable.interactable = value;
        }

        private void OnEnable()
        {
            if (actionProvider != null)
                action = actionProvider.GetAction();

            if (action != null)
                ConnectToAction(action);
        }


        private void OnDisable()
        {
            if (action != null)
                DisconnectToAction(action);
        }

        private void ConnectToAction(InputAction inputAction)
        {
            NameAndParameters[] interactionsNameAndParams;
            if(string.IsNullOrEmpty(inputAction.interactions))
                interactionsNameAndParams = Array.Empty<NameAndParameters>();
            else
            {
                string[] interactions = inputAction.interactions.Split(';');
                interactionsNameAndParams = new NameAndParameters[interactions.Length];
                for (int i = 0; i < interactions.Length; i++)
                    interactionsNameAndParams[i] = NameAndParameters.Parse(interactions[i]);
            }
        }

        private void DisconnectToAction(InputAction inputAction)
        {

        }


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(Interactable)
                onClick.Invoke(eventData);
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            if(Interactable)
                onClick.Invoke(eventData);
        }
    }
}