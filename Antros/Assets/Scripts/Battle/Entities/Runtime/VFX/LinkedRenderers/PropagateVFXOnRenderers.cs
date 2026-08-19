using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core.Cutscenes;
using ATCG.Battle.CapacitySystem.Core.Cutscenes.Elements;
using ATCG.Battle.CapacitySystem.Core.Properties;
using ATCG.Battle.CapacitySystem.Status.Status;
using ATCG.Battle.CapacitySystem.Status.Status.Components;
using Helteix.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Timeline;

using ATCG.Cutscenes;
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
        void ICutsceneElement.Connect(ICutsceneContext context)
        {
            if (!context.TryGetProperty(CutsceneContextKeys.CASTER, out ICutsceneActor caster) || caster == null)
                return;

            Current = caster;

        }

        void ICutsceneElement.Disconnect()
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

            // Same approach as ParticleSystemBehaviour / Unity's Control Track: advance by delta
            // when playing forward, resimulate from 0 on a backward jump, and NEVER Pause. A
            // stationary playhead gives delta 0 (no advance), so nothing drifts; leaving systems
            // paused is what let Unity's Scene particle preview ("Show Only Selected") hijack them.
            foreach (var p in particleSystems)
            {
                if (p == null) continue;

                if (delta < 0f)
                    p.Simulate((float)time, true, true, false); // backward/jump: resimulate 0 -> time
                else
                    p.Simulate(delta, true, false, false);      // forward: advance by delta
            }
        }

        void ITimeControl.OnControlTimeStart()
        {
            if (source != null && Current != null)
            {
                Generate();

                // Reset the clock and fix the seed so resimulations stay deterministic (no
                // per-frame reseed flicker). SetTime drives the systems from here on.
                lastTime = 0d;
                foreach (var p in particleSystems)
                    if (p != null)
                        p.useAutoRandomSeed = false;
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
            {
                // Editor-only probe: the caster comes from the injected CASTER; if it never
                // arrived, Generate silently produced nothing, which reads as "doesn't work
                // in the editor". Check the test environment's HeroAnimator binding.
                if (!Application.isPlaying)
                    Debug.LogWarning("[PropagateVFX] No caster connected (CASTER not injected) — nothing generated.", this);
                return;
            }

            if(source == null)
                return;

            // Instances are parented to 'container'. If it's null they spawn in the active
            // scene instead of the preview stage scene, so the preview camera never renders
            // them — invisible in the editor, fine at runtime (no scene isolation).
            if (!Application.isPlaying && container == null)
                Debug.LogWarning("[PropagateVFX] 'container' is unassigned — spawned VFX land outside the " +
                                 "preview scene and won't render in the Capacity Editor.", this);

            LinkedRendererGroup models = Current.Models;
            IEnumerable<LinkedRenderer> renderers = keys != LinkedRendererKey.None ? models.GetAllFor(keys) : models.GetAll();

            int spawnedBefore = particleSystems.Count;

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

            // Caster resolved but no renderer matched the key → nothing spawned.
            if (!Application.isPlaying && particleSystems.Count == spawnedBefore)
                Debug.LogWarning($"[PropagateVFX] Caster has no LinkedRenderer matching key '{keys}' — no VFX spawned.", this);
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

        private void Clear()
        {
            if (Application.isPlaying)
            {
                // Hand each instance to the persistent reaper instead of fading it on this
                // component: the cutscene GameObject (and this component) is destroyed the
                // moment the cutscene ends, so a fade owned here either got cut short or
                // orphaned instances in the scene. The reaper outlives the cutscene and
                // guarantees the fade completes and the instance is destroyed.
                foreach (var p in particleSystems)
                    VFXReaper.Reap(p);
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