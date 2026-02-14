using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ATCG.UI
{
    public class CustomSelectable : Selectable
    {
        public event Action<AxisEventData> OnMoved;
        public event Action<BaseEventData> OnSelected;
        public event Action<BaseEventData> OnDeselected;
        public event Action<PointerEventData> OnPointerEntered;
        public event Action<PointerEventData> OnPointerExited;
        public event Action<PointerEventData> OnPointerWasDown;
        public event Action<PointerEventData> OnPointerWasUp;

        [SerializeField]
        private bool selectOnClick = false;

        private bool lockInteractable = false;


        public override bool IsInteractable()
        {
            return base.IsInteractable() && !lockInteractable;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!selectOnClick)
                lockInteractable = true;

            base.OnPointerDown(eventData);
            lockInteractable = false;
            OnPointerWasDown?.Invoke(eventData);
        }


        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            OnPointerWasUp?.Invoke(eventData);
        }

        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
            OnMoved?.Invoke(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            OnSelected?.Invoke(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            OnDeselected?.Invoke(eventData);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            OnPointerEntered?.Invoke(eventData);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            OnPointerExited?.Invoke(eventData);
        }

    }
}