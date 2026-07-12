using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Battle.Entities.Runtime.Status;
using Helteix.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Timeline;

namespace ATCG.Battle.Entities.Runtime.VFX
{
    public class PropagateVFXOnRenderers : MonoBehaviour,
        IRuntimeStatusComponent,
        ICapacityCutsceneElement,
        ITimeControl
    {
        [SerializeField]
        private Transform container;
        [Space]
        [SerializeField]
        private ParticleSystem source;
        [SerializeField, EnumToggleButtons]
        private LinkedRendererKey keys;


        private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>();

        public ILinkedRendererSource Current { get; private set; }

        private double lastTime;


        // Pulls the caster actor from the context (game phase or editor preview) and
        // caches its skinned renderers. No runtime/World coupling, no editor-only path:
        // the context abstracts where the actor comes from.
        void ICapacityCutsceneElement.Connect(ICapacityContext context)
        {
            if (!context.TryGetProperty(CapacityContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;

            Current = caster;

        }

        void ICapacityCutsceneElement.Disconnect()
        {
            Clear();
        }

        void IRuntimeStatusComponent.OnApplyStatus(RuntimeStatusContext context)
        {
            Current = context.runtimeEntity;
            Generate();
        }

        void IRuntimeStatusComponent.OnRemoveStatus(RuntimeStatusContext context)
        {
            Clear();
        }

        void IRuntimeStatusComponent.OnTickStatus(RuntimeStatusContext context)
        {

        }
        void ITimeControl.SetTime(double time)
        {
            float delta = (float)(time - lastTime);
            lastTime = time;

            foreach (var p in particleSystems)
            {
                if (p == null) continue;

                if (delta < 0f)
                {
                    p.Simulate((float)time, true, true, false);
                }
                else
                {
                    p.Simulate(delta, true, false, false);
                }
            }
        }

        void ITimeControl.OnControlTimeStart()
        {
            if (source != null && Current != null)
            {
                Generate();
            }
        }

        void ITimeControl.OnControlTimeStop()
        {
            Clear();
        }

        private void Reset()
        {
            keys = LinkedRendererKey.Body;
        }

        private void Generate()
        {
            if(Current == null)
                return;

            if(source == null)
                return;

            LinkedRendererGroup models = Current.Models;
            IEnumerable<LinkedRenderer> renderers = keys != LinkedRendererKey.None ? models.GetAllFor(keys) : models.GetAll();

            foreach (LinkedRenderer model in renderers)
            {
                Renderer entityRenderer = model.Renderer;
                switch (entityRenderer)
                {
                    case MeshRenderer meshRenderer:
                    {
                        ParticleSystem instance = source.InstantiatePrefab(container);
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
                        var instance = source.InstantiatePrefab(container);
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

        // Toggles new emission on every instance this component currently manages, without
        // touching particles already alive — used by PropagateVFXBehaviour's ease-out
        // window (Timeline keeps calling SetTime/Simulate on the same instances, so
        // whatever's already emitted keeps dying naturally instead of being cut off).
        // Re-enabling (scrubbing back before the hold point) resumes normal emission.
        public void SetEmissionEnabled(bool enabled)
        {
            foreach (var p in particleSystems)
            {
                if (p == null)
                    continue;

                ParticleSystem.EmissionModule emission = p.emission;
                emission.enabled = enabled;
            }
        }

        private static async Awaitable StopParticleSystems(ParticleSystem particleSystem)
        {
            if (particleSystem == null)
                return;

            // Sort du mode "paused" laissé par Simulate : réactive la simulation autonome.
            particleSystem.Play(true);
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            while (particleSystem != null && particleSystem.particleCount > 0)
            {
                await Awaitable.NextFrameAsync();
            }

            if (particleSystem)
                particleSystem.DestroyGameObject();
        }

        private void Clear()
        {
            if (Application.isPlaying)
            {
                foreach (var p in particleSystems)
                    StopParticleSystems(p).ListenForExceptions();
            }
            else
            {
                foreach (var p in particleSystems)
                    if (p != null)
                        DestroyImmediate(p.gameObject);
            }

            particleSystems.Clear();
        }

        private void OnDestroy() => Clear();

    }
}