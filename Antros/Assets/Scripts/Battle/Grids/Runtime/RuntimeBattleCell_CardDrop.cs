using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Metrics;
using ATCG.Battle.Players;
using Helteix.Cards.UI.Physical.Drag;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.HexGrids.Runtime
{
    public partial class RuntimeBattleCell : ICardDropTarget<IBattleCard>
    {
        int ICardDropTarget<IBattleCard>.Priority => 1;

        bool ICardDropTarget<IBattleCard>.Accepts(IBattleCard cardUICurrent)
        {
            if (BattleGrid.IsInDeployPhase)
            {
                
            }
        }

        void ICardDropTarget<IBattleCard>.OnCardEnter(IBattleCard cardUI)
        {

        }

        void ICardDropTarget<IBattleCard>.OnCardExit(IBattleCard cardUI)
        {

        }

        void ICardDropTarget<IBattleCard>.OnCardDrop(IBattleCard cardUI)
        {

        }

        void ICardDropTarget<IBattleCard>.OnCardHover(IBattleCard cardUICurrent)
        {

        }
    }
}