using UnityEngine.EventSystems;

namespace Helteix.Cards.UI.Physical.Components
{
    public interface ICardSubmitHandler : ICardUIComponent
    {
        void OnCancelCard();
        void OnSubmitCard();
    }
}