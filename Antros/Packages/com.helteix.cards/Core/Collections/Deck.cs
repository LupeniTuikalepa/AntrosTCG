using System.Collections.Generic;

namespace Helteix.Cards.Collections
{
    public class Deck<TCard> : CardCollection<TCard> where TCard : ICard
    {
        public int CurrentSize => cards.Count;

        private readonly List<TCard> cards = new(16);
        private readonly HashSet<TCard> cardSet = new();

        public override IEnumerable<TCard> Cards => cards;

        protected override bool AddCard(TCard card)
        {
            if (!cardSet.Add(card))
                return false;

            cards.Add(card);
            return true;
        }

        protected override bool RemoveCard(TCard card)
        {
            if (!cardSet.Remove(card))
                return false;

            cards.Remove(card);
            return true;
        }

        public bool TryGet(out TCard card)
        {
            if (TryPeek(out card))
                return true;

            card = default;
            return false;
        }

        public bool TryPeek(out TCard card)
        {
            if (cards.Count == 0)
            {
                card = default;
                return false;
            }

            card = cards[0];
            return true;
        }
    }
}