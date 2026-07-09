using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Runtime;
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
        [SerializeField, BoxGroup("UI")]
        private TMP_Text deployableName;

        [field: SerializeField, BoxGroup("GameFeel"),]
        public Animator Animator { get; private set; }
        
        public override async Awaitable Spawn(RuntimeEntityManager manager, DeployableAspect aspect)
        {
            await base.Spawn(manager, aspect);
            deployableName.text = "Deployable"; //TODO replace with DeployableName

            manager.RegisterRuntimeEntity(this);

            if (RuntimeBattleGrid.TryGetBattleCellAt(aspect.GridMemberComponent.coordinates, out RuntimeBattleCell cell))
            {
                transform.position = cell.transform.position;

                Tween.StopAll(transform);
                await Tween.PunchScale(transform, Vector3.one * -2, .25f);
            }
        }

        public void Despawn(RuntimeEntityManager manager)
        {
            manager.UnregisterRuntimeEntity(this);
        }
    }
}