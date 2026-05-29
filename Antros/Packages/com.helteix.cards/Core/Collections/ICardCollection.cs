using System;
using System.Collections.Generic;

namespace Helteix.Cards.Collections
{
    public interface ICardCollection<TCard> : ICardContainer<TCard> where TCard : ICard
    {
        event Action<TCard> OnCardAdded;
        event Action<TCard> OnCardRemoved;
        IEnumerable<TCard> Cards { get; }
    }
}