using System.Collections.Generic;
using Helteix.Cards.Collections;
using UnityEngine.Pool;

namespace Helteix.Cards
{
    public static class CardExtensions
    {
        public static void Shuffle<TCard>(this CardCollection<TCard> collection)
            where TCard : ICard
        {
            using (ListPool<TCard>.Get(out var list))
            {
                list.AddRange(collection.Cards);
                foreach (var card in list)
                    collection.TryRemoveCard(card);

                // Fisher-Yates — guarantees all permutations are equally probable, O(n).
                for (int i = list.Count - 1; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    (list[i], list[j]) = (list[j], list[i]);
                }

                foreach (var card in list)
                    collection.TryAddCard(card);
            }
        }

        public static void Transfer<TCard>(this CardCollection<TCard> collection, params TCard[] cards)
            where TCard : ICard
        {
            for (int i = 0; i < cards.Length; i++)
                collection.TryAddCard(cards[i]);
        }

        public static void Transfer<TCard>(this CardCollection<TCard> from, CardCollection<TCard> to)
            where TCard : ICard
        {
            using (ListPool<TCard>.Get(out var list))
            {
                list.AddRange(from.Cards);
                foreach (var card in list)
                    to.TryAddCard(card);
            }
        }
    }
}