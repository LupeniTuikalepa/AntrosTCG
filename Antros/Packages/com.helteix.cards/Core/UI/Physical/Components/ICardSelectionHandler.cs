using UnityEngine;
using UnityEngine.EventSystems;

namespace Helteix.Cards.UI.Physical.Components
{
    public interface ICardSelectionHandler : ICardUIComponent
    {
        void OnSelect();
        void OnDeselect();
        void OnMove(Vector2 direction);
    }
}