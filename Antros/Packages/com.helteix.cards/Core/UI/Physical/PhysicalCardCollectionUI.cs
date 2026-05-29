using System;
using System.Linq;
using System.Collections.Generic;
using Helteix.Cards.UI.Physical.Components;
using Helteix.Cards.UI.Physical.Drag;
using UnityEngine;
using UnityEngine.EventSystems;
using Helteix.Tools;
using Helteix.Tools.Phases;

namespace Helteix.Cards.UI.Physical
{
    public partial class PhysicalCardCollectionUI<TCard> : CardCollectionUI<TCard>, IPhysicalCardCollectionUI
        where TCard : ICard
    {

        IEnumerable<CardHolderUI> IPhysicalCardCollectionUI.Holders => holders.Values;


        [Header("References")]
        [SerializeField, Tooltip("If null, the current EventSystem will be used")]
        private EventSystem customEventSystem;

        [Header("Holders")]
        [SerializeField]
        private CardFactoryUI factory;
        [SerializeField, Space]
        private CardHolderUI holderPrefab;
        [SerializeField]
        private Transform containerRoot;

        [field: Header("Dragging")]
        [field: SerializeField]
        public RectTransform DraggingParent { get; private set; }

        [field: SerializeField]
        public bool Use3DDragging { get; private set; } = true;
        private Transform ContainerRoot => containerRoot ? containerRoot : transform;

        private Canvas canvas;
        private Dictionary<TCard, CardHolderUI> holders;

        private Dictionary<CardHolderUI, CardDragPhase<TCard>> drags;


        public EventSystem EventSystem => customEventSystem == null ? EventSystem.current : customEventSystem;
        protected override void Awake()
        {
            base.Awake();

            canvas = GetComponentInParent<Canvas>();
            holders = new Dictionary<TCard, CardHolderUI>();
            drags = new Dictionary<CardHolderUI, CardDragPhase<TCard>>();
        }

        public void SetEventSystem(EventSystem eventSystem) => customEventSystem = eventSystem;

        protected override void OnCardAdded(TCard card)
        {
            base.OnCardAdded(card);

            if (!factory.TryGetUIForCard(card, out CardUI<TCard> cardUI))
                return;

            CardHolderUI holder = Instantiate(holderPrefab, ContainerRoot);
            holders.Add(card, holder);
            cardUI.SetUIInfos(this, holder);

            factory.Activate(cardUI, holder.transform);

            holder.Init(cardUI, this);

            try
            {
                cardUI.Connect(card);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        protected override void OnCardRemoved(TCard card)
        {
            base.OnCardRemoved(card);
            if (factory.TryGetUIForCard(card, out CardUI<TCard> cardUI) && cardUI.CollectionUI == this)
            {
                cardUI.SetUIInfos(null, null);

                try
                {
                    cardUI.Disconnect();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                factory.Return(cardUI);

                if (holders.Remove(card, out CardHolderUI container))
                {
                    container.Dispose();
                    Destroy(container.gameObject);
                }
            }
        }

        protected override void ConnectWithCurrent()
        {
            containerRoot.ClearChildren();
            base.ConnectWithCurrent();
        }

        protected override void DisconnectWithCurrent()
        {
            containerRoot.ClearChildren();
            base.DisconnectWithCurrent();
        }

        public bool TryGetHolderFor(TCard card, out CardHolderUI holder) => holders.TryGetValue(card, out holder);

        public bool TryGetDragPhase(TCard card, out CardDragPhase<TCard> phase)
        {
            if(TryGetHolderFor(card, out CardHolderUI holder))
                return drags.TryGetValue(holder, out phase);

            phase = null;
            return false;
        }

        private Camera GetCanvasCamera()
        {
            Camera canvasCamera = canvas.rootCanvas ? canvas.rootCanvas.worldCamera : canvas.worldCamera;
            return canvasCamera ? canvasCamera : Camera.main;
        }
    }
}