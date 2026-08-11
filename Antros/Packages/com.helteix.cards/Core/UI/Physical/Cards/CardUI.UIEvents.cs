using UnityEngine;
using UnityEngine.EventSystems;

namespace Helteix.Cards.UI.Physical
{
    /// <summary>
    /// The card itself is the Unity EventSystem surface (raycast target).
    /// Every callback is forwarded to the collection, keyed on this card's <see cref="CardHolderUI"/>,
    /// so the whole collection / drag API stays keyed on the holder and is left untouched.
    /// <para/>
    /// Sub-elements of the card keep their own Unity handlers (Button, IPointerClickHandler, ...):
    /// Unity's native event bubbling routes a click on a sub-widget to that widget when it has a
    /// handler, and lets a press-drag started anywhere on the card bubble up here so the card still
    /// drags normally.
    /// </summary>
    public abstract partial class CardUI<TCard> :
        ISelectHandler, IDeselectHandler, IMoveHandler,
        ISubmitHandler, ICancelHandler,
        IPointerClickHandler,
        IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler,
        IInitializePotentialDragHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
        where TCard : ICard
    {
        // The card only routes events once it has been added to a collection (holder + collection set).
        private bool CanReceiveCardEvents => CollectionUI != null && HolderUI != null;

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.SelectCard(HolderUI);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.DeselectCard(HolderUI);
        }

        void IMoveHandler.OnMove(AxisEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.MoveCardSelection(HolderUI, eventData.moveVector);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.BeginCardHover(HolderUI, eventData.position);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.EndCardHover(HolderUI, eventData.position);
        }

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.MoveCardHover(HolderUI, eventData.position, eventData.delta);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.ClickCard(HolderUI, eventData.button);
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.SubmitCard(HolderUI);
        }

        void ICancelHandler.OnCancel(BaseEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.CancelCard(HolderUI);
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.InitializePotentialCardDrag(HolderUI);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.BeginCardDrag(HolderUI);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.UpdateCardDrag(HolderUI, eventData.position, eventData.delta);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (CanReceiveCardEvents) CollectionUI.EndCardDrag(HolderUI);
        }
    }
}
