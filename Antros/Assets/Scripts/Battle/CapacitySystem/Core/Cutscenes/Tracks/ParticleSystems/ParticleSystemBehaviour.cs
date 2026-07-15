using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Drives one clip's referenced ParticleSystem through three phases regardless of
    /// whatever Main.loop/Duration is authored on it — the clip is the single source of
    /// truth for the system's lifetime.
    ///
    /// Driven via ParticleSystem.Simulate (like PropagateVFXOnRenderers.SetTime), not
    /// Play() — Play() relies on Unity's normal per-frame update to advance the sim,
    /// which only ticks in Play Mode. In the Timeline editor's edit-mode preview
    /// (Capacity Editor scrubbing) there is no such update, so a Play()-driven system
    /// just sits static. Simulate() is deterministic from the clip's own local time, so
    /// it scrubs correctly both in Play Mode and in edit-mode preview.
    ///   1. Clip start: Clear + reset the local time accumulator.
    ///   2. Every frame: advance the sim by the clip-local time delta (or hard re-simulate
    ///      from 0 when scrubbing backward). Past duration - EaseOutDuration (the clip's
    ///      Ease Out handle), emission is disabled — no new particles, but everything
    ///      already alive keeps simulating/dying on its own timing since Simulate keeps
    ///      running. Scrubbing back before that point re-enables emission.
    ///   3. Clip exit (natural end, scrub-away, or an interrupted cutscene — anything that
    ///      ends this clip's active window): Stop(StopEmittingAndClear) + deactivate.
    /// </summary>
    public sealed class ParticleSystemBehaviour : PlayableBehaviour
    {
        public ParticleSystem ParticleSystem;
        public double EaseOutDuration;

        private double lastTime;
        private bool emissionEnabled;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (ParticleSystem == null)
                return;

            ParticleSystem.gameObject.SetActive(true);
            ParticleSystem.Clear(true);

            lastTime = 0d;
            emissionEnabled = true;
            SetEmissionEnabled(true);

            // Kicks the system into Simulate-driven mode at t=0 instead of Play()'s
            // real-time mode — PrepareFrame takes over from here every frame.
            ParticleSystem.Simulate(0f, true, true, false);
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (ParticleSystem == null)
                return;

            double time = playable.GetTime();
            double holdEnd = playable.GetDuration() - EaseOutDuration;

            bool shouldEmit = time < holdEnd;
            if (shouldEmit != emissionEnabled)
            {
                emissionEnabled = shouldEmit;
                SetEmissionEnabled(shouldEmit);
            }

            float delta = (float)(time - lastTime);
            lastTime = time;

            if (delta < 0f)
                ParticleSystem.Simulate((float)time, true, true, false);
            else
                ParticleSystem.Simulate(delta, true, false, false);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (ParticleSystem == null)
                return;

            ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.gameObject.SetActive(false);
        }

        private void SetEmissionEnabled(bool enabled)
        {
            ParticleSystem.EmissionModule emission = ParticleSystem.emission;
            emission.enabled = enabled;
        }
    }
}
