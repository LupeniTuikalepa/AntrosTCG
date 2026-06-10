using System;
using ATCG.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public abstract class EntityActionUIButton : EntityActionUIElement
    {
        public bool IsActive { get; private set; }

        private CustomButtonUI button;

        protected override void Awake()
        {
            base.Awake();
            if(!TryGetComponent(out button))
                button = GetComponentInChildren<CustomButtonUI>();
        }

        private void OnEnable()
        {
            button.OnClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            button.OnClick.RemoveListener(OnClick);
        }

        public void BuildButton()
        {
            button.Interactable = IsButtonInteractable();
            IsActive = Build();
            gameObject.SetActive(IsActive);
        }

        protected virtual bool IsButtonInteractable() => true;
        protected abstract bool Build();


        protected abstract void OnClick(BaseEventData baseEventData);
    }
}