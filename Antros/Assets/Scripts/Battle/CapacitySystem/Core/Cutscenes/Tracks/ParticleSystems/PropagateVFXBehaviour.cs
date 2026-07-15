using ATCG.Battle.Entities.Runtime.VFX;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Drives one clip's referenced PropagateVFXOnRenderers through the ITimeControl
    /// contract it already implements (the same one a native Control Track would call),
    /// but resolved per-clip via ExposedReference instead of a track binding — mirrors
    /// ParticleSystemClip/ParticleSystemBehaviour so the two tracks behave the same way
    /// from the Timeline author's point of view.
    ///   - Clip start: OnControlTimeStart() — propagator spawns one instance per matching
    ///     LinkedRenderer (per its own keys) and starts driving them via SetTime/Simulate.
    ///   - Every frame: SetTime(playable time) keeps every spawned instance in sync with
    ///     the clip's local time, scrub-safe in both directions.
    ///   - Outside [Ease In, duration - Ease Out] — read live from the TimelineClip every
    ///     frame, so it always matches the drag handles with no copied/stale value —
    ///     emission is disabled on every spawned instance while SetTime keeps being
    ///     called, so already-alive particles keep simulating/dying naturally instead of
    ///     being cut off. Scrubbing back into the window re-enables emission.
    ///   - Clip exit (natural end, scrub-away, interrupted cutscene): OnControlTimeStop() —
    ///     the propagator's own Clear() takes over (real Stop(StopEmitting) + async
    ///     destroy once every particle is dead).
    /// </summary>
    public sealed class PropagateVFXBehaviour : PlayableBehaviour
    {
        public PropagateVFXOnRenderers Propagator;
        public TimelineClip Clip;

        private ITimeControl control;
        private bool emissionEnabled;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (Propagator == null)
                return;

            control = Propagator;
            emissionEnabled = true;
            control.OnControlTimeStart();
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (control == null)
                return;

            double time = playable.GetTime();
            control.SetTime(time);

            double duration = Clip != null ? Clip.duration : playable.GetDuration();
            double easeIn = Clip != null ? Clip.easeInDuration : 0d;
            double easeOut = Clip != null ? Clip.easeOutDuration : 0d;

            bool shouldEmit = time >= easeIn && time < duration - easeOut;
            if (shouldEmit != emissionEnabled)
            {
                emissionEnabled = shouldEmit;
                Propagator.SetEmissionEnabled(emissionEnabled);
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (control == null)
                return;

            control.OnControlTimeStop();
            control = null;
        }
    }
}
