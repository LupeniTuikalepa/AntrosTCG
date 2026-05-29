
using Helteix.Cards.UI.Physical.Drag;
using UnityEngine;

namespace Helteix.Cards.UI.Physical.Components
{
    public interface ICardPointerDragHandler : ICardUIComponent
    {
        void OnInitializePotentialCardDrag();

        void OnUpdateCardDrag(Vector2 position, Vector2 delta);

        void OnBeginCardDrag();

        void OnEndCardDrag();
        void OnCardDrop<TCard>(Vector3 screenPosition, ICardDropTarget<TCard> resultTarget) where TCard : ICard;
    }
}