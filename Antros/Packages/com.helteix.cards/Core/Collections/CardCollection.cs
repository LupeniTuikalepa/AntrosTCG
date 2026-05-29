using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Helteix.Cards.Collections
{
    public abstract class CardCollection<TCard> : ICardCollection<TCard> where TCard : ICard
    {
        public event Action<TCard> OnCardAdded;
        public event Action<TCard> OnCardRemoved;
        public abstract IEnumerable<TCard> Cards { get; }

        public bool TryAddCard(TCard card, bool notify = true)
        {
            if (AddCard(card))
            {
                if (card.Container is ICardContainer<TCard> cardContainer)
                    cardContainer.TryRemoveCard(card, notify);

                card.Container = this;
                if(notify)
                    OnCardAdded?.Invoke(card);
                return true;
            }

            return false;
        }

        public bool TryRemoveCard(TCard card, bool notify = true)
        {
            if (!RemoveCard(card))
                return false;

            card.Container = null;
            if(notify)
                OnCardRemoved?.Invoke(card);
            return true;

        }

        public void Clear(bool notify = true)
        {
            using (ListPool<TCard>.Get(out List<TCard> cards))
            {
                cards.AddRange(Cards);
                foreach (var card in cards)
                    TryRemoveCard(card, notify);
            }
        }

        protected abstract bool AddCard(TCard card);
        protected abstract bool RemoveCard(TCard card);
    }
}