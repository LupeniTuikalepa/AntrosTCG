using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ATCG.Capacities;
using Helteix.Cards;
using Helteix.Tools.UI;

namespace ATCG.Cards
{
    public abstract class GameCard<T> : Card, IGameCard where T : GameCardData
    {
        IEnumerable<CapacityData> IUIListSource<CapacityData>.Items => Capacities;

        public event Action<CapacityData> ItemAdded;

        public event Action<CapacityData> ItemRemoved;

        GameCardData IGameCard.CardData => Data;
        public string Title => Data.Title;
        public string Description => Data.Description;
        public int InvocationCost => Data.InvocationCost;

        public IEnumerable<CapacityData> Capacities
        {
            get
            {
                foreach (var capacity in Data.Capacities)
                    yield return capacity;
                foreach (var capacity in additionalCapacities)
                    yield return capacity;
            }
        }

        private HashSet<CapacityData> additionalCapacities;
        public T Data { get; }

        protected GameCard(T data)
        {
            Data = data;
            additionalCapacities = new();
        }

        public void AddCapacity(CapacityData capacityData)
        {
            if(additionalCapacities.Add(capacityData))
                ItemAdded?.Invoke(capacityData);
        }

        public void RemoveCapacity(CapacityData capacityData)
        {
            if (additionalCapacities.Remove(capacityData))
                ItemRemoved?.Invoke(capacityData);
        }
    }
}