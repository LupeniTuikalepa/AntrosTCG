using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PrimeTween;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Runtime
{
    public partial class RuntimeBattleCell : ICardDropTarget<IBattleCard>
    {
        public Priority<Color> CellColor { get; private set; }
        public Priority<Vector3> CellSize { get; private set; }

        [Header("Deploy")]
        [SerializeField]
        private float hoverScaleMultiplier = 1.15f;
        [SerializeField]
        private float invalidScaleMultiplier = .9f;
        [SerializeField]
        private Color validDeployCellColor = Color.green;
        [SerializeField]
        private Color invalidDeployCellColor = Color.red;
        [SerializeField]
        private Color cardHoverCellColor = Color.white;

        public IBattleCellLookupPhase CurrentLookupPhase => RuntimeBattleGrid.CurrentLookupPhase;


        public void RefreshLookupPhaseStatus(IBattleCellLookupPhase last, IBattleCellLookupPhase current)
        {
            if (last != null)
            {
                CellColor.RemovePriority(last.ChannelKey);
                CellSize.RemovePriority(last.ChannelKey);
            }

            if (current != null)
            {
                bool isValid = current.IsCoordValid(Coordinates);
                PriorityTags priority = isValid ? PriorityTags.Smallest : PriorityTags.High;

                CellColor.AddPriority(current.ChannelKey, priority, isValid ? validDeployCellColor : invalidDeployCellColor);
                CellSize.AddPriority(current.ChannelKey, priority,  Vector3.one * (isValid ? 1 : invalidScaleMultiplier));
            }
        }

        int ICardDropTarget<IBattleCard>.Priority => 1;

        bool ICardDropTarget<IBattleCard>.Accepts(ICardUI<IBattleCard> cardUI)
        {
            return CurrentLookupPhase != null && CurrentLookupPhase.IsCoordValid(Coordinates);
        }

        void ICardDropTarget<IBattleCard>.OnCardEnter(ICardUI<IBattleCard> cardUI)
        {
            if(CurrentLookupPhase == null)
                return;

            CellSize.AddPriority(cardUI.HolderUI, PriorityTags.Default, Vector3.one * hoverScaleMultiplier);
            CellColor.AddPriority(cardUI.HolderUI, PriorityTags.Default, cardHoverCellColor);
        }

        void ICardDropTarget<IBattleCard>.OnCardExit(ICardUI<IBattleCard> cardUI)
        {
            if(CurrentLookupPhase == null)
                return;

            CellSize.RemovePriority(cardUI.HolderUI);
            CellColor.RemovePriority(cardUI.HolderUI);
        }

        void ICardDropTarget<IBattleCard>.OnCardDrop(ICardUI<IBattleCard> cardUI)
        {
            if(CurrentLookupPhase == null)
                return;

            CellSize.RemovePriority(cardUI.HolderUI);
            CellColor.RemovePriority(cardUI.HolderUI);

            CurrentLookupPhase.SetResult(BattleCell);
        }

        void ICardDropTarget<IBattleCard>.OnCardHover(ICardUI<IBattleCard> cardUI)
        {

        }

    }
}