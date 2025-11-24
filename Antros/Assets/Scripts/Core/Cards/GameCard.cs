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
        IEnumerable<ICapacityDescriptions> IUIListSource<ICapacityDescriptions>.Items => Capacities;

        public event Action<ICapacityDescriptions> ItemAdded;

        public event Action<ICapacityDescriptions> ItemRemoved;

        GameCardData IGameCard.CardData => Data;
        public string Title => Data.Title;
        public string Description => Data.Description;
        public IEnumerable<ICapacityDescriptions> Capacities
        {
            get
            {
                foreach (var capacity in Data.Capacities)
                    yield return capacity;
                foreach (var capacity in additionalCapacities)
                    yield return capacity;
            }
        }

        private HashSet<ICapacityDescriptions> additionalCapacities;
        public T Data { get; }

        protected GameCard(T data)
        {
            Data = data;
            additionalCapacities = new();
        }

        public void AddCapacity(ICapacityDescriptions capacityDescriptions)
        {
            if(additionalCapacities.Add(capacityDescriptions))
                ItemAdded?.Invoke(capacityDescriptions);
        }

        public void RemoveCapacity(ICapacityDescriptions capacityDescriptions)
        {
            if (additionalCapacities.Remove(capacityDescriptions))
                ItemRemoved?.Invoke(capacityDescriptions);
        }
    }
}