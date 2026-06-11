using System;
using ATCG.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ATCG.Battle.Entities.Runtime.UI
{
    public abstract class EntityActionUIButton: EntityActionUIElement
    {

        protected CustomButtonUI button;

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


        protected virtual bool IsButtonInteractable() => true;

        protected abstract void OnClick(BaseEventData baseEventData);
    }
}