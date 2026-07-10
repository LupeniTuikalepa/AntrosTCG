using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime
{
    public abstract partial class RuntimeEntity<T> :
        IPointerClickHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (IsSelected)
                    UnSelect();
                else
                    Select();
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Manager.BeginHover(this);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Manager.EndHover(this);
        }

        protected virtual void OnHovered()
        {
        }

        protected virtual void OnUnhovered()
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