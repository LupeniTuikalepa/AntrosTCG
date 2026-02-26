using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Players.Local.Phases;
using Helteix.Cards.UI.Physical;
using Helteix.Cards.UI.Physical.Drag;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Runtime
{
    public partial class RuntimeBattleCell : ICardDropTarget<IBattleCard>,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
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

        public SelectCellPhase CurrentSelectPhase => RuntimeBattleGrid.CurrentLookupPhase;


        public void RefreshLookupPhaseStatus(SelectCellPhase last, SelectCellPhase current)
        {
            if (last != null)
            {
                CellColor.RemovePriority(last.MainChannelKey);
                CellSize.RemovePriority(last.MainChannelKey);

                CellColor.RemovePriority(last.SecondaryChannelKey);
                CellSize.RemovePriority(last.SecondaryChannelKey);
            }

            if (current != null)
            {
                bool isValid = current.IsCoordValid(Coordinates);
                PriorityTags priority = isValid ? PriorityTags.Smallest : PriorityTags.High;

                CellColor.AddPriority(current.MainChannelKey, priority, isValid ? validDeployCellColor : invalidDeployCellColor);
                CellSize.AddPriority(current.MainChannelKey, priority,  Vector3.one * (isValid ? 1 : invalidScaleMultiplier));
            }
        }

        int ICardDropTarget<IBattleCard>.Priority => 1;

        bool ICardDropTarget<IBattleCard>.Accepts(ICardUI<IBattleCard> cardUI)
        {
            return CurrentSelectPhase != null && CurrentSelectPhase.IsCoordValid(Coordinates);
        }

        void ICardDropTarget<IBattleCard>.OnCardEnter(ICardUI<IBattleCard> cardUI)
        {
            if(CurrentSelectPhase == null)
                return;

            CellSize.AddPriority(CurrentSelectPhase.SecondaryChannelKey, PriorityTags.High, Vector3.one * hoverScaleMultiplier);
            CellColor.AddPriority(CurrentSelectPhase.SecondaryChannelKey, PriorityTags.High, cardHoverCellColor);
        }

        void ICardDropTarget<IBattleCard>.OnCardExit(ICardUI<IBattleCard> cardUI)
        {
            if(CurrentSelectPhase == null)
                return;

            CellSize.RemovePriority(CurrentSelectPhase.SecondaryChannelKey);
            CellColor.RemovePriority(CurrentSelectPhase.SecondaryChannelKey);
        }

        void ICardDropTarget<IBattleCard>.OnCardDrop(ICardUI<IBattleCard> cardUI)
        {
            if(CurrentSelectPhase == null)
                return;

            CellSize.RemovePriority(CurrentSelectPhase.SecondaryChannelKey);
            CellColor.RemovePriority(CurrentSelectPhase.SecondaryChannelKey);

            CurrentSelectPhase.SetResult(BattleCell);
        }

        void ICardDropTarget<IBattleCard>.OnCardHover(ICardUI<IBattleCard> cardUI)
        {
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if(CurrentSelectPhase == null)
                return;

            if (CurrentSelectPhase.IsCoordValid(Coordinates))
            {
                CellSize.AddPriority(CurrentSelectPhase.SecondaryChannelKey, PriorityTags.High, Vector3.one * hoverScaleMultiplier);
                CellColor.AddPriority(CurrentSelectPhase.SecondaryChannelKey, PriorityTags.High, cardHoverCellColor);
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if(CurrentSelectPhase == null)
                return;
            if (CurrentSelectPhase.IsCoordValid(Coordinates))
            {
                CellSize.RemovePriority(CurrentSelectPhase.SecondaryChannelKey);
                CellColor.RemovePriority(CurrentSelectPhase.SecondaryChannelKey);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(CurrentSelectPhase == null)
                return;

            CellSize.RemovePriority(CurrentSelectPhase.SecondaryChannelKey);
            CellColor.RemovePriority(CurrentSelectPhase.SecondaryChannelKey);

            if(CurrentSelectPhase.IsCoordValid(Coordinates))
                CurrentSelectPhase?.SetResult(BattleCell);
        }
    }
}