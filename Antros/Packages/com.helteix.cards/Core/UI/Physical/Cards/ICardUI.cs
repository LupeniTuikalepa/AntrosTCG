using System.Collections.Generic;
using Helteix.Cards.UI.Physical.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Helteix.Cards.UI.Physical
{
    public interface ICardUI<out TCard> : ICardUI where TCard : ICard
    {
        ICard ICardUI.Card => Card;

        new TCard Card { get; }

    }

    public interface ICardUI
    {
        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get; }
        // ReSharper disable once InconsistentNaming
        Transform transform { get; }
        RectTransform RectTransform { get; }
        CanvasGroup CanvasGroup { get; }
        Canvas CardCanvas { get; }
        ICard Card { get; }
        IPhysicalCardCollectionUI CollectionUI { get; }
        CardHolderUI HolderUI { get; }
        void Disconnect();
        bool TryGetCardComponent<T>(out T component) where T : ICardUIComponent;
        IEnumerable<T> GetCardComponents<T>() where T : ICardUIComponent;
        void RegisterComponent(ICardUIComponent cardUIComponent);
        void UnregisterComponent(ICardUIComponent cardUIComponent);
    }
}