using System;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public class RuntimeStatusMaterial : RuntimeStatusComponent
    {
        [SerializeField]
        private Material material;
        
        private Material[] defaultMaterials;

        public override void OnApplyStatus(RuntimeStatusContext context)
        {
            var meshRenderer = context.runtimeEntity.gameObject.GetComponentInChildren<MeshRenderer>();
            var temp = new Material[meshRenderer.materials.Length + 1];
            temp = meshRenderer.materials;
            temp[^1] = material;
            meshRenderer.materials = temp;
        }

        public override void OnRemoveStatus(RuntimeStatusContext context)
        {
            var meshRenderer = context.runtimeEntity.gameObject.GetComponentInChildren<MeshRenderer>();
            meshRenderer.materials = defaultMaterials;
        }

        public override void OnTickStatus(RuntimeStatusContext context)
        {
        }
    }
}