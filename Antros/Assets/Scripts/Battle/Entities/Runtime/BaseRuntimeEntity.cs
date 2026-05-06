using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime
{
    public abstract class RuntimeEntity<T> : MonoBehaviour,
        IRuntimeEntity,
        IPointerClickHandler,
        IPointerEnterHandler, IPointerExitHandler
        where T : IEntityAspect
    {
        public event Action<T> OnEntityConnected;
        public event Action<T> OnEntityDisconnected;

        public event Action OnEntitySelected;
        public event Action OnEntityDeselected;


        public EntityAddress Address => Aspect.EntityAddress;

        public T Aspect { get; private set; }
        public RuntimeEntityManager Manager { get; private set; }

        public bool IsSelected => Manager.IsSelected(this);

        public bool IsHovered { get; private set; }


        public virtual void Connect(RuntimeEntityManager manager, T aspect)
        {
            Aspect = aspect;
            Manager = manager;

            OnEntityConnected?.Invoke(aspect);
        }


        public virtual void Disconnect()
        {
            T last = Aspect;

            Aspect = default;
            Manager = null;

            OnEntityDisconnected?.Invoke(last);
        }


        public void Select() => Manager.Select(this);

        public void UnSelect() => Manager.Unselect(this);


        void IRuntimeEntity.OnSelected()
        {
            OnSelected();
            OnEntitySelected?.Invoke();
        }

        void IRuntimeEntity.OnDeselected()
        {
            OnDeselected();
            OnEntityDeselected?.Invoke();
        }

        void IRuntimeEntity.SetInteractableState(bool isInMask)
        {
            SetInteractableState(isInMask);

            if(!isInMask && IsSelected)
                UnSelect();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(IsSelected)
                UnSelect();
            else
                Select();

            OnPointerClick(eventData);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            IsHovered = true;
            OnPointerEnter(eventData);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
            OnPointerExit(eventData);
        }

        protected virtual void OnPointerClick(PointerEventData pointerEventData) { }
        protected virtual void OnPointerEnter(PointerEventData pointerEventData) { }
        protected virtual void OnPointerExit(PointerEventData pointerEventData) { }

        protected virtual void OnSelected() { }
        protected virtual void OnDeselected() { }
        protected virtual void SetInteractableState(bool isInMask) { }

    }
}