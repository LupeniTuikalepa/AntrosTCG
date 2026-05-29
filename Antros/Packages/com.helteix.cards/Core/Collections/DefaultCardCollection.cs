using System.Collections.Generic;
using System.Linq;

namespace Helteix.Cards.Collections
{
    public class DefaultCardCollection<TCard> : CardCollection<TCard>
        where TCard : ICard
    {
        private List<TCard> cards = new(16);
        public override IEnumerable<TCard> Cards => cards;

        protected override bool AddCard(TCard card)
        {
            if (cards.Contains(card))
                return false;

            cards.Add(card);
            return true;
        }

        protected override bool RemoveCard(TCard card) => cards.Remove(card);
    }
}