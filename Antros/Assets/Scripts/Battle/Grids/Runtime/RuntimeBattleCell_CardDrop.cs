using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.Tools.Phases;

namespace ATCG.Battle.Grids.Runtime
{
    public partial class RuntimeBattleCell : ICardDropTarget<IBattleCard>,
        IPhaseListener<PlayerDragBattleCardPhase>
    {
        #region Phases

        private List<PlayerDragBattleCardPhase> dragCardPhase;

        void IPhaseListener<PlayerDragBattleCardPhase>.OnPhaseBegin(PlayerDragBattleCardPhase phase)
        {
            dragCardPhase.Add(phase);
        }

        void IPhaseListener<PlayerDragBattleCardPhase>.OnPhaseEnd(PlayerDragBattleCardPhase phase)
        {
            dragCardPhase.Remove(phase);
        }

        #endregion

        #region Drag and drop

        int ICardDropTarget<IBattleCard>.Priority => 1;

        bool ICardDropTarget<IBattleCard>.Accepts(IBattleCard cardUICurrent)
        {
            return true;
        }

        void ICardDropTarget<IBattleCard>.OnCardEnter(IBattleCard card)
        {

        }

        void ICardDropTarget<IBattleCard>.OnCardExit(IBattleCard card)
        {

        }

        void ICardDropTarget<IBattleCard>.OnCardDrop(IBattleCard card)
        {
            foreach (PlayerDragBattleCardPhase phase in dragCardPhase)
            {
                if (phase.card == card && phase.IsCoordValid(Coordinates))
                    phase.SetResult(BattleCell);
            }
        }

        void ICardDropTarget<IBattleCard>.OnCardHover(IBattleCard card)
        {

        }

        #endregion

    }
}