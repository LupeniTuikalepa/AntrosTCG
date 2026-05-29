using Helteix.Cards;
using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Components;
using Helteix.Cards.UI.Physical.Drag;
using PrimeTween;
using UnityEngine;

namespace ATCG.Cards.UI.Components

{
    [AddComponentMenu("ATCG/Cards/Card Effects")]
    public class GameCardUIEffects : CardUIComponent<IGameCard>,
        ICardPointerHoverHandler, ICardPointerDragHandler
    {
        public Canvas CardCanvas => CardUI.CardCanvas;
        public CanvasGroup CanvasGroup => CardUI.CanvasGroup;

        private int lastSortingOrder;

        void ICardPointerHoverHandler.OnBeginCardHover()
        {

        }

        void ICardPointerHoverHandler.OnEndCardHover()
        {

        }

        void ICardPointerHoverHandler.OnCardHoverMove(Vector2 position, Vector2 delta)
        {

        }

        void ICardPointerDragHandler.OnInitializePotentialCardDrag()
        {
        }

        void ICardPointerDragHandler.OnUpdateCardDrag(Vector2 position, Vector2 delta)
        {
        }

        void ICardPointerDragHandler.OnBeginCardDrag()
        {
            lastSortingOrder = CardCanvas.sortingOrder;
            CardCanvas.sortingOrder = 100;
            Tween.StopAll(CanvasGroup);
            Tween.Alpha(CanvasGroup, .2f, .25f);
        }

        void ICardPointerDragHandler.OnEndCardDrag()
        {
            CardCanvas.sortingOrder = lastSortingOrder;

            Tween.StopAll(CanvasGroup);
            Tween.Alpha(CanvasGroup, 1f, .25f);
        }

        void ICardPointerDragHandler.OnCardDrop<TCard>(Vector3 screenPosition, ICardDropTarget<TCard> resultTarget)
        {
        }
    }
}