using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Drives one clip's referenced ParticleSystem through three phases regardless of
    /// whatever Main.loop/Duration is authored on it — the clip is the single source of
    /// truth for the system's lifetime:
    ///   1. Clip start: Clear + Play.
    ///   2. duration - EaseOutDuration (the clip's Ease Out handle): Stop(StopEmitting) —
    ///      no new particles, but everything already alive keeps simulating/dying on its
    ///      own timing.
    ///   3. Clip exit (natural end, scrub-away, or an interrupted cutscene — anything that
    ///      ends this clip's active window): Stop(StopEmittingAndClear) + deactivate.
    /// </summary>
    public sealed class ParticleSystemBehaviour : PlayableBehaviour
    {
        public ParticleSystem ParticleSystem;
        public double EaseOutDuration;

        private bool transitionStarted;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (ParticleSystem == null)
                return;

            transitionStarted = false;

            ParticleSystem.gameObject.SetActive(true);
            ParticleSystem.Clear(true);
            ParticleSystem.Play(true);
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (ParticleSystem == null || transitionStarted)
                return;

            double duration = playable.GetDuration();
            double holdEnd = duration - EaseOutDuration;

            if (playable.GetTime() >= holdEnd)
            {
                transitionStarted = true;
                ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (ParticleSystem == null)
                return;

            ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.gameObject.SetActive(false);
        }
    }
}
