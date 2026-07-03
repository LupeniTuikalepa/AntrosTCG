using System;
using ATCG.Capacities.Data.Status;
using UnityEngine;
using UnityEngine.Serialization;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public class RuntimeStatusVFX : RuntimeStatusComponent
    {
        [SerializeField]
        private ParticleSystem[] particleSystems;

        private Renderer[] entityRenderers;

        public override void OnApplyStatus(RuntimeStatusContext context)
        {
            entityRenderers = context.renderers;

            switch (entityRenderers)
            {   /*
                case MeshRenderer[] meshRenderers:
                {
                    foreach (var system in particleSystems)
                    {
                        var shape = system.shape;
                        foreach (var meshRenderer in meshRenderers)
                        {
                            shape.meshRenderer = meshRenderer;
                        }
                    }
                    break;
                }
                case SkinnedMeshRenderer[] skinnedMeshRenderer:
                {
                    foreach (var system in particleSystems)
                    {
                        var shape = system.shape;
                        shape.skinnedMeshRenderer = skinnedMeshRenderer;
                    }
                    break;
                }
                */
            }
        }

        public override void OnRemoveStatus(RuntimeStatusContext context)
        {
        }

        public override void OnTickStatus(RuntimeStatusContext context)
        {
        }
    }
}