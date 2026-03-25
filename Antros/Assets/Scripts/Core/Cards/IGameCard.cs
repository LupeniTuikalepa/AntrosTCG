using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ATCG.Capacities;
using Helteix.Cards;
using Helteix.Tools.UI;
using UnityEngine;

namespace ATCG.Cards
{
    public interface IGameCard : ICard, IUIListSource<CapacityData>
    {
        GameCardData CardData { get; }
        IEnumerable<CapacityData> CapacitiesData { get; }

        string Title { get; }
        string Description { get; }
        int InvocationCost { get; }
    }
}