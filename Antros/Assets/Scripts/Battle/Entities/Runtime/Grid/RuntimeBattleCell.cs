using ATCG.Battle.Cards;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids.Runtime;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using PrimeTween;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Runtime.Grid
{
    public partial class RuntimeBattleCell : RuntimeEntity<BattleCellAspect>
    {
        public HexCoordinates Coordinates => Aspect.GridMemberComponent.coordinates;


        public override async Awaitable Spawn(RuntimeEntityManager manager, BattleCellAspect aspect)
        {
            await base.Spawn(manager, aspect);

            await Awaitable.EndOfFrameAsync();

            using (ListPool<Material>.Get(out var list))
            {
                bool found = false;

                BattlePhase battlePhase = BattlePhase;
                foreach (IBattlePlayer player in battlePhase.Players)
                {
                    foreach (HexCoordinates coord in player.GetStartingLine())
                    {
                        if (coord == Coordinates)
                        {
                            Material modelMaterial = RuntimeBattleGrid.GetCellPlayerMaterial(player);
                            list.Add(modelMaterial);
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        break;
                }
                if(!found)
                    list.Add(RuntimeBattleGrid.CellMaterial);

                list.Add(RuntimeBattleGrid.CellMaterial);

                Model.SetMaterials(list);
            }
            transform.localScale = Vector3.zero;
            float delay = Coordinates.Length() * .2f;
            Easing overshoot = Easing.Overshoot(.3f);

            await Tween.Scale(transform, RuntimeBattleGrid.GetTargetScale(), .3f, overshoot, startDelay: delay);
        }
    }
}