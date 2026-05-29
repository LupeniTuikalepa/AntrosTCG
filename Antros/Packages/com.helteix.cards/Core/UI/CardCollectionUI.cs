using Helteix.Cards.Collections;
using UnityEngine;

namespace Helteix.Cards.UI
{
    public abstract class CardCollectionUI<TCard> : MonoBehaviour
        where TCard : ICard
    {
        protected ICardCollection<TCard> Current { get; private set; }

        protected virtual void Awake()
        {

        }

        public void Connect(ICardCollection<TCard> collection)
        {
            Disconnect();
            Current = collection;

            ConnectWithCurrent();

            collection.OnCardAdded += OnCardAdded;
            collection.OnCardRemoved += OnCardRemoved;

            foreach (TCard card in collection.Cards)
                OnCardAdded(card);
        }

        public void Disconnect()
        {
            if (Current == null)
                return;

            foreach (TCard card in Current.Cards)
                OnCardRemoved(card);

            Current.OnCardAdded -= OnCardAdded;
            Current.OnCardRemoved -= OnCardRemoved;

            DisconnectWithCurrent();
            Current = null;
        }

        protected virtual void OnCardAdded(TCard card) { }
        protected virtual void OnCardRemoved(TCard card) { }
        protected virtual void ConnectWithCurrent() { }
        protected virtual void DisconnectWithCurrent() { }
    }
}