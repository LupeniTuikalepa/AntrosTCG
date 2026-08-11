using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ATCG.Capacities;
using ATCG.Metrics;
using ATCG.Passives.Datas;
using Helteix.Cards;
using Helteix.Tools.UI;

namespace ATCG.Cards
{
    public abstract class GameCard<T> : Card, IGameCard where T : GameCardData
    {

        GameCardData IGameCard.CardData => Data;
        public string Title => Data.Title;
        public string Description => Data.Description;

        public int InvocationCost => GameMetrics.Current.CardRarityInvocationCost.TryGetValueForKey(Data.Rarity, out int value) ?
            value :
            0;

        public IEnumerable<CapacityData> Capacities => Data.Capacities.GetCapacities();
        public IEnumerable<PassiveData> Passives => Data.Passives;
        public T Data { get; }

        protected GameCard(T data)
        {
            Data = data;
        }

    }
}