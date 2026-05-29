using UnityEngine.EventSystems;

namespace Helteix.Cards.UI.Physical.Components
{
    public interface ICardPointerClickHandler : ICardUIComponent
    {
        void OnClickCard(PointerEventData.InputButton buttonID);
    }
}