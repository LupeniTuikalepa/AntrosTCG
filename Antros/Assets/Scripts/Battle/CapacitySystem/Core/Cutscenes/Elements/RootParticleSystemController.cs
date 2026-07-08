using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Properties;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements
{
    public class RootParticleSystemController : MonoBehaviour,
        ICapacityCutsceneElement,
        ITimeControl
    {
        private SkinnedMeshRenderer[] skinnedMeshRenderers;

        [SerializeField]
        private ParticleSystem source;

        [SerializeField]
        private Transform container;

        private List<ParticleSystem> particleSystems = new List<ParticleSystem>();

        // Pulls the caster actor from the context (game phase or editor preview) and
        // caches its skinned renderers. No runtime/World coupling, no editor-only path:
        // the context abstracts where the actor comes from.
        void ICapacityCutsceneElement.Connect(ICapacityContext context)
        {
            if (!context.TryGetProperty(CapacityContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;

            using (ListPool<SkinnedMeshRenderer>.Get(out var list))
            {
                Renderer[] models = caster.Models;
                for (int i = 0; i < models.Length; i++)
                {
                    if (models[i] is SkinnedMeshRenderer skinnedMeshRenderer)
                        list.Add(skinnedMeshRenderer);
                }

                skinnedMeshRenderers = list.ToArray();
            }
        }

        public void Disconnect()
        {
            Clear();
        }

        private double lastTime;

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
            if (source != null && skinnedMeshRenderers != null)
            {
                Clear();
                for (int i = 0; i < skinnedMeshRenderers.Length; i++)
                {
                    ParticleSystem instance = Instantiate(source, container);
                    instance.gameObject.SetActive(true);
                    instance.gameObject.hideFlags = HideFlags.DontSave;

                    ParticleSystem.ShapeModule shape = instance.shape;
                    shape.skinnedMeshRenderer = skinnedMeshRenderers[i];

                    particleSystems.Add(instance);
                }
            }
        }

        void ITimeControl.OnControlTimeStop()
        {
            Clear();
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