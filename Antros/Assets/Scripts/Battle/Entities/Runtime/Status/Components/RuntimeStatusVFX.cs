using System;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public class RuntimeStatusVFX : RuntimeStatusComponent
    {
        [SerializeField]
        private new ParticleSystem particleSystem;

        public override void OnApplyStatus(RuntimeStatusContext context)
        {
            var shape = particleSystem.shape;
            shape.meshRenderer = context.runtimeEntity.gameObject.GetComponentInChildren<MeshRenderer>();
        }

        public override void OnRemoveStatus(RuntimeStatusContext context)
        {
        }

        public override void OnTickStatus(RuntimeStatusContext context)
        {
        }
    }
}