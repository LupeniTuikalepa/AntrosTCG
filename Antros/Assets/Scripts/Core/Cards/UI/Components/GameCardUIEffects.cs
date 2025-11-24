using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Components;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Cards.UI.Components

{
    [AddComponentMenu("ATCG/Cards/Card Effects")]
    public class GameCardUIEffects : CardUIComponent<IGameCard>,
        ICardPointerHoverHandler, ICardPointerDragHandler
    {
        public Canvas CardCanvas => CardUI.CardCanvas;
        public CanvasGroup CanvasGroup => CardUI.CanvasGroup;

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }

        public void OnPointerMove(PointerEventData eventData)
        {

        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            CardCanvas.sortingOrder = 100;
            Tween.StopAll(CanvasGroup);
            Tween.Alpha(CanvasGroup, .2f, .25f);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CardCanvas.sortingOrder = -3;

            Tween.StopAll(CanvasGroup);
            Tween.Alpha(CanvasGroup, 1f, .25f);
        }
    }
}