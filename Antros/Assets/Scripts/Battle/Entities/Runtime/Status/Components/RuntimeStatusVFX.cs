using System.Collections.Generic;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public class RuntimeStatusVFX : RuntimeStatusComponent
    {
        [SerializeField]
        private GameObject vfxPrefab;
        
        [SerializeField]
        private Transform container;

        private Renderer[] entityRenderers;
        
        private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

        private void Awake()
        {
            if (container == null)
                container = transform;
        }

        public override void OnApplyStatus(RuntimeStatusContext context)
        {
            entityRenderers = context.renderers;

            int count = entityRenderers.Length;
    
            for (int i = 0; i < count; i++)
            {
                var entityRenderer = entityRenderers[i];
                switch (entityRenderer)
                {
                    case MeshRenderer meshRenderer:
                    {
                        var instance = Instantiate(vfxPrefab, container);
                        instance.gameObject.SetActive(true);
                        instance.gameObject.hideFlags = HideFlags.DontSave;

                        var systems = instance.GetComponentsInChildren<ParticleSystem>();
                        for (int j = 0; j < systems.Length; j++)
                        {
                            var system = systems[j];
                            ParticleSystem.ShapeModule shape = system.shape;
                            shape.shapeType = ParticleSystemShapeType.MeshRenderer;
                            shape.meshRenderer = meshRenderer;
                            particleSystems.Add(system);
                        }
                        break;
                    }
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                    {
                        var instance = Instantiate(vfxPrefab, container);
                        instance.gameObject.SetActive(true);
                        instance.gameObject.hideFlags = HideFlags.DontSave;

                        var systems = instance.GetComponentsInChildren<ParticleSystem>();
                        for (int j = 0; j < systems.Length; j++)
                        {
                            var system = systems[j];
                            ParticleSystem.ShapeModule shape = system.shape;
                            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                            shape.skinnedMeshRenderer = skinnedMeshRenderer;
                            particleSystems.Add(system);
                        }
                        break;
                    }
                }

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