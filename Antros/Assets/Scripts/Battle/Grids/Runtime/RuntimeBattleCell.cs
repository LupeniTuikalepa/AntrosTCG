using System;
using ATCG.Battle.Metrics;
using ATCG.Battle.Players.Local.Phases;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PrimeTween;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Runtime
{
    public partial class RuntimeBattleCell : MonoBehaviour
    {
        public BattleCell BattleCell { get; private set; }
        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }
        public RuntimeHexGrid RuntimeGrid => RuntimeBattleGrid.RuntimeHexGrid;
        public HexCoordinates Coordinates => BattleCell.cell.coordinates;

        [field: SerializeField]
        public SpriteRenderer SpriteRenderer { get; private set; }

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


        public void Connect(RuntimeBattleGrid grid, BattlePhase phase, BattleCell battleCell)
        {
            if (BattleCell != null)
                Disconnect(phase, battleCell);

            RuntimeBattleGrid = grid;
            BattleCell = battleCell;
            transform.localScale = Vector3.zero;
            Tween.Scale(transform, GetTargetScale(), .3f, Easing.Overshoot(.3f),
                startDelay: Coordinates.Length() * .2f);
        }

        public Vector3 GetTargetScale()
        {
            return Vector3.one * RuntimeGrid.Current.OuterCellRadius * 1.8f;
        }

        public void Disconnect(BattlePhase phase, BattleCell battleCell)
        {
            if (BattleCell != battleCell)
                return;

            BattleCell = null;
        }

    }
}