using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
using ATCG.HexGrids.Utility;
using Helteix.Tools;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

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