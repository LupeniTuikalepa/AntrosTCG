using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.HexGrids.Utility;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Deployables
{
    public partial class RuntimeDeployable : RuntimeEntity<DeployableAspect>
    {
        [field: SerializeField, BoxGroup("GameFeel"),]
        public Animator Animator { get; private set; }
        
        public override async Awaitable Spawn(RuntimeEntityManager manager, DeployableAspect aspect)
        {
            await base.Spawn(manager, aspect);

            manager.RegisterRuntimeEntity(this);

            if (RuntimeBattleGrid.TryGetBattleCellAt(aspect.GridMemberComponent.coordinates, out RuntimeBattleCell cell))
            {
                transform.position = cell.transform.position;

                Tween.StopAll(transform);
                await Tween.PunchScale(transform, Vector3.one * -2, .25f);
            }

            if (manager.TryGetRuntimeEntity(aspect.DeployableEntityTag.caster, out var runtimeEntity))
            {
                HexOperations.ComputeQuaternion(
                                    transform.position,
                                    runtimeEntity.transform.position,
                                    out var deployableTargetRotation);
                await Tween.Rotation(transform, deployableTargetRotation, 0, Ease.InOutQuint);
            }
        }

        public void Despawn(RuntimeEntityManager manager)
        {
            manager.UnregisterRuntimeEntity(this);
        }
    }
}