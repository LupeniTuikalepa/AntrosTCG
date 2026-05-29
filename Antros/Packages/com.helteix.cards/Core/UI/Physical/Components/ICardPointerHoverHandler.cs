using UnityEngine;

namespace Helteix.Cards.UI.Physical.Components
{
    public interface ICardPointerHoverHandler : ICardUIComponent
    {
        void OnBeginCardHover();

        void OnEndCardHover();

        void OnCardHoverMove(Vector2 position, Vector2 delta);
    }
}