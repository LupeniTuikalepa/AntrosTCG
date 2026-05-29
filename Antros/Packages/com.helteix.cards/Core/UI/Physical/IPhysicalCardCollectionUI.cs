using System.Collections.Generic;
using Helteix.Cards.UI.Physical.Drag;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Helteix.Cards.UI.Physical
{
    public interface IPhysicalCardCollectionUI
    {
        bool CanCardBeHovered(ICard card);
        bool CanCardBeClicked(ICard card);
        bool CanCardBeDragged(ICard card);
        bool CanCardBeSelected(ICard card);
        bool CanCardBeSubmitted(ICard card);

        IEnumerable<CardHolderUI> Holders { get; }
        RectTransform DraggingParent { get; }
        EventSystem EventSystem { get; }

        bool SelectCard(CardHolderUI cardHolderUI);
        bool DeselectCard(CardHolderUI cardHolderUI);
        bool MoveCardSelection(CardHolderUI cardHolderUI, Vector2 eventDataMoveVector);
        bool BeginCardHover(CardHolderUI cardHolderUI, Vector2 eventDataPosition);
        bool EndCardHover(CardHolderUI cardHolderUI, Vector2 eventDataPosition);
        bool MoveCardHover(CardHolderUI cardHolderUI, Vector2 eventDataPosition, Vector2 eventDataDelta);
        bool ClickCard(CardHolderUI cardHolderUI, PointerEventData.InputButton eventDataButton);
        bool SubmitCard(CardHolderUI cardHolderUI);
        bool CancelCard(CardHolderUI cardHolderUI);
        bool InitializePotentialCardDrag(CardHolderUI cardHolderUI);
        bool UpdateCardDrag(CardHolderUI cardHolderUI, Vector2 position, Vector2 delta);
        bool BeginCardDrag(CardHolderUI cardHolderUI);
        bool EndCardDrag(CardHolderUI cardHolderUI);

    }
}