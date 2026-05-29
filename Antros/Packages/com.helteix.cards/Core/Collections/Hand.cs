using System;
using System.Collections.Generic;
using UnityEngine;

namespace Helteix.Cards.Collections
{
    [Serializable]
    public class Hand<TCard> : CardCollection<TCard>
        where TCard : ICard
    {
        public event Action<Hand<TCard>> OnCardOrderChanged;

        [SerializeReference]
        private List<TCard> cards;

        [field: SerializeField]
        public int MaxSize { get; private set; }

        public override IEnumerable<TCard> Cards => cards;
        public int CurrentSize => cards.Count;

        public Hand(int maxSize = -1)
        {
            cards = new List<TCard>(maxSize > 0 ? maxSize : 16);
            MaxSize = maxSize;
        }

        protected override bool AddCard(TCard card)
        {
            if (MaxSize < 0 || CurrentSize < MaxSize)
            {
                cards.Add(card);
                return true;
            }

            return false;
        }

        protected override bool RemoveCard(TCard card) => cards.Remove(card);

        public int GetCardIndex(TCard card) => cards.IndexOf(card);

        public bool TryGetCardIndex(TCard card, out int index)
        {
            index = cards.IndexOf(card);
            return index > -1;
        }

        public bool MoveCardToIndex(TCard card, int newIndex)
        {
            if (TryGetCardIndex(card, out int index))
            {
                cards.RemoveAt(index);
                cards.Insert(newIndex, card); // fix: was inserting at old index
                OnCardOrderChanged?.Invoke(this);
                return true;
            }

            return false;
        }

        public TCard GetCard(int index) => TryGetCard(index, out TCard card) ? card : default;
        public bool TryGetCard(int index, out TCard card)
        {
            if (index >= 0 && index < cards.Count)
            {
                card = cards[index];
                return true;
            }
            card = default;
            return false;
        }

        public bool Sort(IComparer<TCard> comparer)
        {
            cards.Sort(comparer);
            OnCardOrderChanged?.Invoke(this);
            return true;
        }
    }
}