using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ATCG.Capacities;
using Helteix.Cards;
using Helteix.Tools.UI;
using UnityEngine;

namespace ATCG.Cards
{
    public interface IGameCard : ICard, IUIListSource<ICapacityDescriptions>
    {
        GameCardData CardData { get; }
        IEnumerable<ICapacityDescriptions> Capacities { get; }

        string Title { get; }
        string Description { get; }
    }
}