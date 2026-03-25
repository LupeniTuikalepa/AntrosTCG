using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ATCG.Capacities;
using ATCG.Metrics;
using Helteix.Cards;
using Helteix.Tools.UI;

namespace ATCG.Cards
{
    public abstract class GameCard<T> : Card, IGameCard where T : GameCardData
    {
        IEnumerable<CapacityData> IUIListSource<CapacityData>.Items => CapacitiesData;

        public event Action<CapacityData> ItemAdded;
        public event Action<CapacityData> ItemRemoved;

        GameCardData IGameCard.CardData => Data;
        public string Title => Data.Title;
        public string Description => Data.Description;

        public int InvocationCost => GameMetrics.Current.CardRarityInvocationCost.TryGetValueForKey(Data.Rarity, out int value) ?
            value :
            0;

        public IEnumerable<CapacityData> CapacitiesData
        {
            get
            {
                foreach (var capacity in Data.Capacities)
                    yield return capacity;
            }
        }

        public T Data { get; }

        protected GameCard(T data)
        {
            Data = data;
        }

    }
}