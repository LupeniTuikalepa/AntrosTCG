using System;
using System.Collections.Generic;
using Helteix.Cards.UI.Physical.Components;
using Helteix.Cards.UI.Physical.Drag;
using UnityEngine;

namespace Helteix.Cards.UI.Physical
{
    [RequireComponent(typeof(CanvasGroup), typeof(Canvas))]
    public abstract class CardUI<TCard> : MonoBehaviour, ICardUI<TCard>
        where TCard : ICard
    {
        IPhysicalCardCollectionUI ICardUI.CollectionUI => CollectionUI;

        public TCard Card { get; private set; }
        public PhysicalCardCollectionUI<TCard> CollectionUI { get; internal set; }
        public CardHolderUI HolderUI { get; private set; }
        public CardFactoryUI FactoryUI { get; internal set; }

        // Components indexed by interface type for O(1) lookup in GetCardComponents.
        private readonly Dictionary<Type, List<ICardUIComponent>> componentsByType = new();
        // Flat set for deduplication in Connect/Disconnect iterations.
        private readonly HashSet<ICardUIComponent> allComponents = new();

        public Canvas CardCanvas
        {
            get
            {
                if (cardCanvas == null)
                    cardCanvas = GetComponent<Canvas>();
                return cardCanvas;
            }
        }

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (canvasGroup == null)
                    canvasGroup = GetComponent<CanvasGroup>();
                return canvasGroup;
            }
        }

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                    rectTransform = transform as RectTransform;
                return rectTransform;
            }
        }

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Canvas cardCanvas;

        private void Awake()
        {
            ICardUIComponent[] found = GetComponentsInChildren<ICardUIComponent>(true);
            for (int i = 0; i < found.Length; i++)
                RegisterComponent(found[i]);
        }

        internal void SetUIInfos(PhysicalCardCollectionUI<TCard> collectionUI, CardHolderUI holder)
        {
            CollectionUI = collectionUI;
            HolderUI = holder;
        }

        public void Connect(TCard card)
        {
            Disconnect();

            Card = card;
            foreach (var component in allComponents)
                if (component is ICardUIComponent<TCard> c)
                    c.Connect(card);

            ConnectWithCurrent();
        }

        public void Disconnect()
        {
            if (Card == null)
                return;

            foreach (var component in allComponents)
                if (component is ICardUIComponent<TCard> c)
                    c.Disconnect(Card);

            DisconnectWithCurrent();
            Card = default;
        }

        public IEnumerable<T> GetCardComponents<T>() where T : ICardUIComponent
        {
            if (componentsByType.TryGetValue(typeof(T), out var list))
                foreach (var c in list)
                    yield return (T)c;
        }

        public bool TryGetDragPhase(out CardDragPhase<TCard> phase) => CollectionUI.TryGetDragPhase(Card, out phase);

        public CardDragPhase<TCard> GetDragPhase() => TryGetDragPhase(out var phase) ? phase : null;

        public void RegisterComponent(ICardUIComponent cardUIComponent)
        {
            if (!allComponents.Add(cardUIComponent))
                return;

            foreach (Type iface in cardUIComponent.GetType().GetInterfaces())
            {
                if (!typeof(ICardUIComponent).IsAssignableFrom(iface) || iface == typeof(ICardUIComponent))
                    continue;

                if (!componentsByType.TryGetValue(iface, out var list))
                {
                    list = new List<ICardUIComponent>();
                    componentsByType[iface] = list;
                }

                list.Add(cardUIComponent);
            }

            if (Card != null && cardUIComponent is ICardUIComponent<TCard> component)
                component.Connect(Card);
        }

        public void UnregisterComponent(ICardUIComponent cardUIComponent)
        {
            if (!allComponents.Remove(cardUIComponent))
                return;

            foreach (Type iface in cardUIComponent.GetType().GetInterfaces())
            {
                if (!typeof(ICardUIComponent).IsAssignableFrom(iface) || iface == typeof(ICardUIComponent))
                    continue;

                if (componentsByType.TryGetValue(iface, out var list))
                    list.Remove(cardUIComponent);
            }

            if (Card != null && cardUIComponent is ICardUIComponent<TCard> component)
                component.Disconnect(Card);
        }

        public bool TryGetCardComponent<T>(out T component) where T : ICardUIComponent
        {
            if (componentsByType.TryGetValue(typeof(T), out var list) && list.Count > 0)
            {
                component = (T)list[0];
                return true;
            }

            component = default;
            return false;
        }

        protected virtual void ConnectWithCurrent() { }
        protected virtual void DisconnectWithCurrent() { }
    }
}