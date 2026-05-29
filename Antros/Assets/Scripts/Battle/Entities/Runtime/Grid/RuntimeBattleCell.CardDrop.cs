using ATCG.Battle.Cards;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime.Grid
{
    public partial class RuntimeBattleCell : ICardDropTarget<IBattleCard>, IPhaseListener<SelectCellPhase>
    {
        public SelectCellPhase CurrentSelectCellPhase { get; private set; }

        int ICardDropTarget<IBattleCard>.Priority => 1;

        bool ICardDropTarget<IBattleCard>.Accepts(ICardUI<IBattleCard> cardUI)
        {
            return CurrentSelectCellPhase != null && IsInteractable && CurrentSelectCellPhase.IsCoordValid(Aspect.Coordinate);
        }

        void ICardDropTarget<IBattleCard>.OnCardEnter(ICardUI<IBattleCard> cardUI)
        {
            Vector2 mousePosition = cardUI.CollectionUI.EventSystem.currentInputModule.input.mousePosition;

            (this as IPointerEnterHandler).OnPointerEnter(new PointerEventData(cardUI.CollectionUI.EventSystem)
            {
                position = mousePosition,
            });
        }

        void ICardDropTarget<IBattleCard>.OnCardExit(ICardUI<IBattleCard> cardUI)
        {
            Vector2 mousePosition = cardUI.CollectionUI.EventSystem.currentInputModule.input.mousePosition;

            (this as IPointerExitHandler).OnPointerExit(new PointerEventData(cardUI.CollectionUI.EventSystem)
            {
                position = mousePosition,
            });
        }

        void ICardDropTarget<IBattleCard>.OnCardDrop(ICardUI<IBattleCard> cardUI)
        {
            Vector2 mousePosition = cardUI.CollectionUI.EventSystem.currentInputModule.input.mousePosition;

            (this as IPointerExitHandler).OnPointerExit(new PointerEventData(cardUI.CollectionUI.EventSystem)
            {
                position = mousePosition,
            });
        }

        void ICardDropTarget<IBattleCard>.OnCardHover(ICardUI<IBattleCard> cardUI)
        {
        }

        void IPhaseListener<SelectCellPhase>.OnPhaseBegin(SelectCellPhase phase)
        {
            CurrentSelectCellPhase = phase;
        }

        void IPhaseListener<SelectCellPhase>.OnPhaseEnd(SelectCellPhase phase)
        {
            CurrentSelectCellPhase = null;
        }
    }
}