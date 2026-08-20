using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Drives one clip's ParticleSystem the way Unity's own Control Track does — advancing via
    /// ParticleSystem.Simulate to the clip's local time, resimulating from 0 on a backward jump,
    /// and NEVER calling Pause. Leaving a system paused is what let Unity's Scene-view particle
    /// preview ("Show Only Selected") hijack it and hide the other systems; re-simulating to the
    /// exact time every frame freezes it deterministically without that side effect.
    ///
    /// On top of the Control Track behaviour it keeps emission ON only inside the clip's
    /// [Ease In, duration - Ease Out] window: past the fade-out no new particles spawn and
    /// whatever is alive dies on its own timing. A backward scrub resimulates that window in
    /// segments so the dying tail is still correct. Object activation on entry/exit is optional.
    /// </summary>
    public sealed class ParticleSystemBehaviour : PlayableBehaviour
    {
        public ParticleSystem ParticleSystem;
        public TimelineClip Clip;
        public bool HandleObjectActivation;

        private ParticleSystem[] allSystems;
        private float lastTime;
        private float emitStart;
        private float emitEnd;

        private const float Unset = float.MaxValue;
        private const float MaxStep = 100f; // never simulate an absurd span in a single call

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (ParticleSystem == null)
                return;

            allSystems = ParticleSystem.GetComponentsInChildren<ParticleSystem>(true);

            // Deterministic scrubbing: without a fixed seed every resimulation reseeds and the
            // particles flicker differently each frame. The seed can only be set on a stopped
            // system, so stop+clear first in case it was auto-playing.
            foreach (ParticleSystem system in allSystems)
            {
                if (system == null) continue;
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.useAutoRandomSeed = false;
            }

            if (HandleObjectActivation)
                ParticleSystem.gameObject.SetActive(true);

            lastTime = Unset; // force a resimulation from 0 on the first frame
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (ParticleSystem == null || !ParticleSystem.gameObject.activeInHierarchy)
                return;

            float time = (float)playable.GetTime();
            double duration = Clip != null ? Clip.duration : playable.GetDuration();
            emitStart = Clip != null ? (float)Clip.easeInDuration : 0f;
            emitEnd = (float)(duration - (Clip != null ? Clip.easeOutDuration : 0d));

            if (lastTime == Unset || time < lastTime)
                SimulateFromStart(time);
            else
                SimulateForward(time - lastTime, time);

            lastTime = time;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (ParticleSystem == null)
                return;

            ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (HandleObjectActivation)
                ParticleSystem.gameObject.SetActive(false);

            lastTime = Unset;
        }

        // Deterministic resimulation to 'target', emitting only inside [emitStart, emitEnd] so a
        // backward scrub into the tail still shows the correct dying particles.
        private void SimulateFromStart(float target)
        {
            ParticleSystem.Simulate(0f, true, true, false);

            float t = 0f;
            t = Segment(t, Mathf.Min(target, emitStart), false); // pre-roll (usually empty)
            t = Segment(t, Mathf.Min(target, emitEnd), true);    // emitting window
            Segment(t, target, false);                           // fade-out tail: no new particles
        }

        // Forward incremental advance — cheap, matches how the Control Track plays forward.
        private void SimulateForward(float delta, float time)
        {
            SetEmissionEnabled(time >= emitStart && time < emitEnd);
            Step(delta);
        }

        private float Segment(float from, float to, bool emit)
        {
            if (to <= from)
                return from;

            SetEmissionEnabled(emit);
            Step(to - from);
            return to;
        }

        private void Step(float dt)
        {
            while (dt > MaxStep)
            {
                ParticleSystem.Simulate(MaxStep, true, false, false);
                dt -= MaxStep;
            }
            if (dt > 0f)
                ParticleSystem.Simulate(dt, true, false, false);
        }

        private void SetEmissionEnabled(bool enabled)
        {
            if (allSystems == null)
                return;

            foreach (ParticleSystem system in allSystems)
            {
                if (system == null)
                    continue;

                ParticleSystem.EmissionModule emission = system.emission;
                emission.enabled = enabled;
            }
        }
    }
}
