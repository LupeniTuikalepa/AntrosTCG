using Helteix.Cards.UI.Physical.Components;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Helteix.Cards.UI.Physical
{
    public partial class PhysicalCardCollectionUI<TCard>
    {
        public virtual bool CanCardBeDragged(ICard card) => true;
        public virtual bool CanCardBeHovered(ICard card) => true;
        public virtual bool CanCardBeClicked(ICard card) => true;
        public virtual bool CanCardBeSelected(ICard card) => true;
        public virtual bool CanCardBeSubmitted(ICard card) => true;

        protected virtual bool TryGetCardAtDirection(CardHolderUI cardHolderUI, Vector2 moveVector,
            out CardHolderUI holderUI)
        {
            holderUI = null;
            return false;
        }

        public bool SelectCard(CardHolderUI holderUI)
        {
            if (!holderUI.CanBeSelected || !CanCardBeSelected(holderUI.Card))
                return false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardSelectionHandler>())
                handler.OnSelect();

            holderUI.OnSelect();
            return true;
        }

        public virtual bool DeselectCard(CardHolderUI holderUI)
        {
            if (!holderUI.CanBeSelected || !CanCardBeSelected(holderUI.Card))
                return false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardSelectionHandler>())
                handler.OnDeselect();

            holderUI.OnDeselect();
            return true;
        }

        public virtual bool MoveCardSelection(CardHolderUI holderUI, Vector2 direction)
        {
            if (!holderUI.CanBeSelected || !CanCardBeSelected(holderUI.Card))
                return false;

            if (TryGetCardAtDirection(holderUI, direction, out CardHolderUI target) && CanCardBeSelected(target.Card))
            {
                foreach (var handler in holderUI.CardUI.GetCardComponents<ICardSelectionHandler>())
                    handler.OnMove(direction);

                holderUI.OnMove(direction);

                SelectCard(target);
                DeselectCard(holderUI);

                return true;
            }

            return false;
        }

        public virtual bool BeginCardHover(CardHolderUI holderUI, Vector2 position)
        {
            if (!holderUI.CanBeHovered || !CanCardBeHovered(holderUI.Card))
                return false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardPointerHoverHandler>())
                handler.OnBeginCardHover();

            holderUI.OnBeginCardHover();
            return true;
        }

        public virtual bool EndCardHover(CardHolderUI holderUI, Vector2 position)
        {
            if (!holderUI.CanBeHovered || !CanCardBeHovered(holderUI.Card))
                return false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardPointerHoverHandler>())
                handler.OnEndCardHover();

            holderUI.OnEndCardHover();
            return true;
        }

        public virtual bool MoveCardHover(CardHolderUI holderUI, Vector2 position, Vector2 delta)
        {
            if (!holderUI.CanBeHovered || !CanCardBeHovered(holderUI.Card))
                return false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardPointerHoverHandler>())
                handler.OnCardHoverMove(position, delta);

            holderUI.OnCardHoverMove(position, delta);
            return true;
        }

        public virtual bool ClickCard(CardHolderUI holderUI, PointerEventData.InputButton button)
        {
            if (!holderUI.CanBeClicked || !CanCardBeClicked(holderUI.Card))
                return false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardPointerClickHandler>())
                handler.OnClickCard(button);

            holderUI.OnClickCard(button);
            return true;
        }

        public virtual bool SubmitCard(CardHolderUI holderUI)
        {
            if (!holderUI.CanBeSubmitted || !CanCardBeSubmitted(holderUI.Card))
                return false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardSubmitHandler>())
                handler.OnSubmitCard();

            holderUI.OnSubmitCard();
            return true;
        }

        public virtual bool CancelCard(CardHolderUI holderUI)
        {
            if (!holderUI.CanBeSubmitted || !CanCardBeSubmitted(holderUI.Card))
                return false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardSubmitHandler>())
                handler.OnCancelCard();

            holderUI.OnCancelCard();
            return true;
        }

        public virtual bool InitializePotentialCardDrag(CardHolderUI holderUI)
        {
            if (!holderUI.CanBeDragged || !CanCardBeDragged(holderUI.Card))
                return false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardPointerDragHandler>())
                handler.OnInitializePotentialCardDrag();

            holderUI.InitializePotentialCardDrag();
            // Debug.Log("InitializePotentialCardDrag");
            return true;
        }

        public virtual bool BeginCardDrag(CardHolderUI holderUI)
        {
            if (!holderUI.CanBeDragged || !CanCardBeDragged(holderUI.Card))
                return false;
            if (holderUI.CardUI is not CardUI<TCard> cardUI)
                return false;

            // Debug.Log("BeginCardDrag");
            Camera cam = GetCanvasCamera();

            CardDragPhase<TCard> phase = new CardDragPhase<TCard>(new()
            {
                cardUI = cardUI,
                camera = cam,
                eventSystem = EventSystem,
                draggingParent = DraggingParent,
                is3D = Use3DDragging,
            });

            drags.Add(holderUI, phase);
            phase.RunAndForget();

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardPointerDragHandler>())
                handler.OnBeginCardDrag();

            holderUI.OnBeginCardDrag();
            return true;
        }

        public virtual bool UpdateCardDrag(CardHolderUI holderUI, Vector2 position, Vector2 delta)
        {
            // Debug.Log("UpdateCardDrag");
            if (!holderUI.CanBeDragged || !CanCardBeDragged(holderUI.Card))
                return false;

            if (!drags.TryGetValue(holderUI, out CardDragPhase<TCard> currentDragPhase))
                return false;

            currentDragPhase.ScreenPosition = position;
            currentDragPhase.ScreenDelta = delta;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardPointerDragHandler>())
                handler.OnUpdateCardDrag(position, delta);

            holderUI.OnUpdateCardDrag(position, delta);
            return true;
        }

        public virtual bool EndCardDrag(CardHolderUI holderUI)
        {
            // Debug.Log("EndCardDrag");
            if (!holderUI.CanBeDragged || !CanCardBeDragged(holderUI.Card))
                return false;

            if (!drags.TryGetValue(holderUI, out CardDragPhase<TCard> currentDragPhase))
                return false;

            currentDragPhase.IsDragging = false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardPointerDragHandler>())
                handler.OnEndCardDrag();

            holderUI.OnEndCardDrag();
            return true;
        }

        public virtual bool OnCardDrop(CardHolderUI holderUI, DragResult<TCard> result)
        {
            if (!holderUI.CanBeDragged || !CanCardBeDragged(holderUI.Card))
                return false;

            if (!drags.Remove(holderUI))
                return false;

            foreach (var handler in holderUI.CardUI.GetCardComponents<ICardPointerDragHandler>())
                handler.OnCardDrop(result.ScreenPosition, result.Target);

            holderUI.OnCardDrop(result.ScreenPosition, result.Target);
            return true;
        }
    }
}