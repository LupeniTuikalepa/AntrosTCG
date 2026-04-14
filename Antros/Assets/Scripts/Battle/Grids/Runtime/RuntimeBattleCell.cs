using ATCG.Battle.Cards;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.GameModes;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using Helteix.ChanneledProperties.Priorities;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Grids.Runtime
{
    public partial class RuntimeBattleCell : MonoBehaviour
    {
        [field: SerializeField]
        public SpriteRenderer SpriteRenderer { get; private set; }

        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }
        public RuntimeHexGrid RuntimeGrid => RuntimeBattleGrid.RuntimeHexGrid;
        public HexCoordinates Coordinates => BattleCellAspect.BattleCellComponent.coordinates;

        public BattleCellAspect BattleCellAspect { get; private set; }

        private void Awake()
        {
            CellColor = new Priority<Color>(SpriteRenderer.color);
            CellColor.AddOnValueChangeCallback(ctx =>
            {
                Tween.StopAll(SpriteRenderer);
                Tween.Color(SpriteRenderer, ctx, 0.2f, Ease.OutExpo);
            });

            CellSize = new Priority<Vector3>(Vector3.one);
            CellSize.AddOnValueChangeCallback(ctx =>
            {
                Tween.StopAll(SpriteRenderer.transform);
                Tween.Scale(SpriteRenderer.transform, ctx, 0.2f, Ease.OutExpo);
            });
        }


        public void Connect(RuntimeBattleGrid grid, BattlePhase phase, BattleCellAspect battleCellAspect)
        {
            if (BattleCellAspect.IsValid())
                Disconnect(phase, battleCellAspect);

            RuntimeBattleGrid = grid;
            BattleCellAspect = battleCellAspect;
            //battleCellAspect.Attacked += BattleCellOnAttacked;

            transform.localScale = Vector3.zero;

            Tween.Scale(transform, grid.GetTargetScale(), .3f, Easing.Overshoot(.3f),
                startDelay: Coordinates.Length() * .2f);
        }

        public void Disconnect(BattlePhase phase, BattleCellAspect battleCellAspect)
        {
            if (BattleCellAspect.IsNot(battleCellAspect))
                return;

            //BattleCellAspect.Attacked -= BattleCellOnAttacked;
            BattleCellAspect = default;
        }


        private void BattleCellOnAttacked(HeroBattleCard card)
        {
            Tween.StopAll(transform);
            Tween.PunchScale(transform, -Vector3.one * 3, .5f);
        }
    }
}