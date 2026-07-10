using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.HexGrids.Utility;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Constructions
{
    public partial class RuntimeConstruction : RuntimeEntity<ConstructionAspect>
    {
        [field: SerializeField, BoxGroup("GameFeel"),]
        public Animator Animator { get; private set; }
        
        public override async Awaitable Spawn(RuntimeEntityManager manager, ConstructionAspect aspect)
        {
            await base.Spawn(manager, aspect);

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