using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids.Runtime;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Grid
{
    public partial class RuntimeBattleCell : RuntimeEntity<BattleCellAspect>
    {
        public HexCoordinates Coordinates => Aspect.GridMemberComponent.coordinates;


        public override async Awaitable Spawn(RuntimeEntityManager manager, BattleCellAspect aspect)
        {
            await base.Spawn(manager, aspect);
            transform.localScale = Vector3.zero;
            float delay = Coordinates.Length() * .2f;
            Easing overshoot = Easing.Overshoot(.3f);

            await Tween.Scale(transform, RuntimeBattleGrid.GetTargetScale(), .3f, overshoot, startDelay: delay);
        }
    }
}