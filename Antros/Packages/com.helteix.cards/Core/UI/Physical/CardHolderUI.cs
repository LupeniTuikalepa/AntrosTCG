using System;
using System.Collections.Generic;
using Helteix.Cards.UI.Physical.Components;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.Cards.UI.Physical.Movers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Helteix.Cards.UI.Physical
{
    [RequireComponent(typeof(LayoutElement)), RequireComponent(typeof(CanvasGroup))]
    public class CardHolderUI : MonoBehaviour,
        ISelectHandler, IDeselectHandler, IMoveHandler,
        ISubmitHandler, ICancelHandler,
        IPointerClickHandler,
        IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler,
        IInitializePotentialDragHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public IPhysicalCardCollectionUI CollectionUI { get; private set; }
        public ICardUI CardUI { get; private set; }

        public RectTransform RectTransform { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }

        public bool CanBeDragged => IsConnected && CollectionUI.CanCardBeDragged(CardUI.Card);
        public bool CanBeSelected => IsConnected && CollectionUI.CanCardBeSelected(CardUI.Card);
        public bool CanBeHovered => IsConnected && CollectionUI.CanCardBeHovered(CardUI.Card);
        public bool CanBeClicked => IsConnected && CollectionUI.CanCardBeClicked(CardUI.Card);
        public bool CanBeSubmitted => IsConnected && CollectionUI.CanCardBeSubmitted(CardUI.Card);
        public bool IsConnected => CardUI != null && CollectionUI != null;

        public int Index => transform.GetSiblingIndex();
        public ICard Card => CardUI.Card;

        DrivenRectTransformTracker tracker = new DrivenRectTransformTracker();

        [SerializeField]
        protected FollowCardMover defaultMover = new FollowCardMover(15);

        private List<ICardUIMover> movers;
        private void Awake()
        {
            RectTransform = transform as RectTransform;
            CanvasGroup = GetComponent<CanvasGroup>();

            movers = new List<ICardUIMover>() { defaultMover };
        }

        private void OnEnable()
        {
            Canvas.willRenderCanvases += Refresh;

        }

        private void OnDisable()
        {
            Canvas.willRenderCanvases -= Refresh;
        }

        internal void Init(ICardUI cardUI, IPhysicalCardCollectionUI collectionUI)
        {
            CardUI = cardUI;
            CollectionUI = collectionUI;
            tracker.Add(this , cardUI.RectTransform, DrivenTransformProperties.All);
        }

        private void Refresh()
        {
            if (!IsConnected)
                return;
            ICardUIMover currentMover = null;
            foreach (var m in movers)
            {
                if (currentMover == null || currentMover.Priority < m.Priority)
                    currentMover = m;
            }

            currentMover?.MoveCard(this, CardUI);
        }

        public void AddMover(ICardUIMover newMover)
        {
            movers.Add(newMover);
        }

        public void RemoveMover(ICardUIMover mover)
        {
            movers.Remove(mover);
        }

        internal void Dispose()
        {
            CardUI = null;

            CollectionUI = null;

            tracker.Clear();
        }


        protected internal virtual void OnSelect() { }
        protected internal virtual void OnDeselect() { }
        protected internal virtual void OnMove(Vector2 direction) { }

        protected internal virtual void OnBeginCardHover() { }
        protected internal virtual void OnEndCardHover() { }
        protected internal virtual void OnCardHoverMove(Vector2 position, Vector2 delta) { }

        protected internal virtual void OnSubmitCard() { }
        protected internal virtual void OnCancelCard() { }

        protected internal virtual void OnClickCard(PointerEventData.InputButton buttonID) { }

        protected internal virtual void InitializePotentialCardDrag() { }
        protected internal virtual void OnBeginCardDrag() { }
        protected internal virtual void OnEndCardDrag() { }
        protected internal virtual void OnUpdateCardDrag(Vector2 position, Vector2 delta) { }

        protected internal virtual void OnCardDrop<TCard>(Vector3 position, ICardDropTarget<TCard> resultTarget) where TCard : ICard
        { }
        void ISelectHandler.OnSelect(BaseEventData eventData) => CollectionUI.SelectCard(this);

        void IDeselectHandler.OnDeselect(BaseEventData eventData) => CollectionUI.DeselectCard(this);

        void IMoveHandler.OnMove(AxisEventData eventData) => CollectionUI.MoveCardSelection(this, eventData.moveVector);

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) => CollectionUI.BeginCardHover(this, eventData.position);

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) => CollectionUI.EndCardHover(this, eventData.position);

        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData) =>CollectionUI.MoveCardHover(this, eventData.position, eventData.delta);

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData) => CollectionUI.ClickCard(this, eventData.button);

        void ISubmitHandler.OnSubmit(BaseEventData eventData) => CollectionUI.SubmitCard(this);

        void ICancelHandler.OnCancel(BaseEventData eventData) => CollectionUI.CancelCard(this);


        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
            => CollectionUI.InitializePotentialCardDrag(this);

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
            => CollectionUI.BeginCardDrag(this);

        void IDragHandler.OnDrag(PointerEventData eventData) => CollectionUI.UpdateCardDrag(this, eventData.position, eventData.delta);

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)=> CollectionUI.EndCardDrag(this);


    }
}