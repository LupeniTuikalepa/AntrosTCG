using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids.Runtime;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using Helteix.ChanneledProperties.Priorities;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Grid
{
    public partial class RuntimeBattleCell : RuntimeEntity<BattleCellAspect>
    {
        [field: SerializeField]
        public SpriteRenderer SpriteRenderer { get; private set; }

        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }
        public RuntimeHexGrid RuntimeGrid => RuntimeBattleGrid.RuntimeHexGrid;
        public HexCoordinates Coordinates => Aspect.BattleGridElementComponent.coordinates;


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


        public void Spawn(RuntimeBattleGrid grid, BattleCellAspect aspect)
        {
            Connect(grid.EntityManager, aspect);

            RuntimeBattleGrid = grid;
            transform.localScale = Vector3.zero;

            Tween.Scale(transform, grid.GetTargetScale(), .3f, Easing.Overshoot(.3f),
                startDelay: Coordinates.Length() * .2f);
        }

        public void Despawn(RuntimeBattleGrid grid)
        {
            Disconnect();
        }

        private void BattleCellOnAttacked(HeroBattleCard card)
        {
            Tween.StopAll(transform);
            Tween.PunchScale(transform, -Vector3.one * 3, .5f);
        }
    }
}