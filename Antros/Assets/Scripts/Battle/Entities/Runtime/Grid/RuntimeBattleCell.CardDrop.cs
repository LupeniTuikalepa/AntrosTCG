using ATCG.Battle.Cards;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime.Grid
{
    public partial class RuntimeBattleCell : ICardDropTarget<IBattleCard>
    {

        int ICardDropTarget<IBattleCard>.Priority => 1;

        bool ICardDropTarget<IBattleCard>.Accepts(ICardUI<IBattleCard> cardUI)
        {
            //return CurrentSelectCellPhase != null && IsInteractable && CurrentSelectCellPhase.IsCoordValid(Aspect.Coordinate);
            return CurrentSelectEntityPhase != null && CurrentSelectEntityPhase.Accepts(Address);
        }

        void ICardDropTarget<IBattleCard>.OnCardEnter(ICardUI<IBattleCard> cardUI)
        {
        }

        void ICardDropTarget<IBattleCard>.OnCardExit(ICardUI<IBattleCard> cardUI)
        {

        }

        void ICardDropTarget<IBattleCard>.OnCardDrop(ICardUI<IBattleCard> cardUI)
        {
        }

        void ICardDropTarget<IBattleCard>.OnCardHover(ICardUI<IBattleCard> cardUI)
        {
        }

    }
}