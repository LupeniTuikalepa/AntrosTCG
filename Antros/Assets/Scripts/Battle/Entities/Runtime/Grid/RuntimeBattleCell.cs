using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids.Runtime;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using Helteix.ChanneledProperties.Priorities;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ATCG.Battle.Entities.Runtime.Grid
{
    public partial class RuntimeBattleCell : RuntimeEntity<BattleCellAspect>
    {

        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }
        public RuntimeHexGrid RuntimeGrid => RuntimeBattleGrid.RuntimeHexGrid;
        public HexCoordinates Coordinates => Aspect.BattleGridElementComponent.coordinates;



        private void OnValidate()
        {
        }

        protected override void Awake()
        {/*
            CellColor = new Priority<Color>(DisabledColor);
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
*/
            base.Awake();
        }

        public void SetGrid(RuntimeBattleGrid battleGrid)
        {
            RuntimeBattleGrid = battleGrid;
        }

        public override async Awaitable Spawn(RuntimeEntityManager manager, BattleCellAspect aspect)
        {
            await base.Spawn(RuntimeBattleGrid.EntityManager, aspect);
            transform.localScale = Vector3.zero;

            float delay = Coordinates.Length() * .2f;
            Easing overshoot = Easing.Overshoot(.3f);

            await Tween.Scale(transform, RuntimeBattleGrid.GetTargetScale(), .3f, overshoot, startDelay: delay);
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