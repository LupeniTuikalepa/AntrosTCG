using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime
{
    public abstract partial class RuntimeEntity<T> :
        IPointerClickHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerMoveHandler
    {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (IsSelected)
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

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            OnPointerMove(eventData);
        }
        protected virtual void OnPointerClick(PointerEventData eventData)
        {
        }

        protected virtual void OnPointerMove(PointerEventData eventData)
        {

        }

        protected virtual void OnPointerEnter(PointerEventData eventData)
        {
        }

        protected virtual void OnPointerExit(PointerEventData eventData)
        {
        }

        protected virtual void OnSelected()
        {
        }

        protected virtual void OnDeselected()
        {
        }

    }
}