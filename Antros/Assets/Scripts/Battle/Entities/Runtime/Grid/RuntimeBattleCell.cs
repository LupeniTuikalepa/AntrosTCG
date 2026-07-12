using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using ATCG.Metrics;
using PrimeTween;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Runtime.Grid
{
    public partial class RuntimeBattleCell : RuntimeEntity<BattleCellAspect>
    {
        public HexCoordinates Coordinates => Aspect.GridMemberComponent.coordinates;

        [SerializeField]
        private MeshRenderer outline;


        public override async Awaitable Spawn(RuntimeEntityManager manager, BattleCellAspect aspect)
        {
            await base.Spawn(manager, aspect);

            await Awaitable.EndOfFrameAsync();

            bool found = false;
            Material outlineMaterial = RuntimeBattleGrid.CellMaterial;

            BattlePhase battlePhase = BattlePhase;
            foreach (IBattlePlayer player in battlePhase.Players)
            {
                foreach (HexCoordinates coord in player.GetStartingLine())
                {
                    if (coord == Coordinates)
                    {
                        outlineMaterial = RuntimeBattleGrid.GetCellPlayerMaterial(player);
                        found = true;
                        break;
                    }
                }

                if (found)
                    break;
            }

            outline.material = outlineMaterial;
            transform.localScale = Vector3.zero;
            float delay = Coordinates.Length() * .2f;
            Easing overshoot = Easing.Overshoot(.3f);

            await Tween.Scale(transform, RuntimeBattleGrid.GetTargetScale(), .3f, overshoot, startDelay: delay);
        }
    }
}