using System;
using System.Buffers;
using System.Collections.Generic;
using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public class RuntimeStatusMaterial : RuntimeStatusComponent
    {
        private readonly struct RendererAndMaterials
        {
            public readonly Renderer renderer;
            public readonly Material[] defaultMaterials;

            public RendererAndMaterials(Renderer renderer)
            {
                this.renderer = renderer;
                defaultMaterials = renderer.materials;
            }
        }
        
        [SerializeField]
        private Material material;
        
        private List<RendererAndMaterials> rendererAndMaterials;
        
        public override void OnApplyStatus(RuntimeStatusContext context)
        {
            foreach (Renderer entityRenderer in context.renderers)
                rendererAndMaterials.Add(new RendererAndMaterials(entityRenderer));

            foreach (var rendererMaterial in rendererAndMaterials)
            {
                var entityRenderer = rendererMaterial.renderer;
                var entityMaterials = rendererMaterial.renderer.materials;
                
                var array = ArrayPool<Material>.Shared.Rent(entityMaterials.Length + 1);
                
                for (int i = 0; i < entityMaterials.Length; i++)
                    array[i] = entityRenderer.materials[i];
                
                array[^1] = material;
                entityRenderer.materials = array;
                
                ArrayPool<Material>.Shared.Return(array);
            }
        }

        public override void OnRemoveStatus(RuntimeStatusContext context)
        {
            foreach (var entityRenderer in rendererAndMaterials)
            {
                entityRenderer.renderer.materials = entityRenderer.defaultMaterials;
            }
        }

        public override void OnTickStatus(RuntimeStatusContext context)
        {
        }
    }
}