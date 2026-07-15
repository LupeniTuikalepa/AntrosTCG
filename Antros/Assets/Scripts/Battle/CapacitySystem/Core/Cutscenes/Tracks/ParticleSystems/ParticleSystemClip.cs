using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Points to its ParticleSystem via an ExposedReference (like CinemachineShot), so
    /// several clips can each target a different system with no track binding. Ease In /
    /// Ease Out (the native Timeline clip handles — draggable in the timeline and
    /// editable as plain numbers in the clip Inspector) bracket the "emitting" window:
    /// before Ease In and past duration - Ease Out, emission is off while already-alive
    /// particles keep dying naturally; the system is fully stopped and deactivated only
    /// once the clip is actually exited.
    /// </summary>
    public sealed class ParticleSystemClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<ParticleSystem> particleSystem;

        [Tooltip("If checked, the clip activates the ParticleSystem's GameObject on entry " +
                 "and deactivates it on exit. Leave off if something else owns activation " +
                 "(e.g. it's already active/managed elsewhere) and the clip should only drive emission.")]
        public bool handleObjectActivation = true;

        // Injected by ParticleSystemTrack.CreateTrackMixer before CreatePlayable runs.
        // Kept as a live TimelineClip reference rather than copied doubles: the drag
        // handles mutate this same object directly, so reading it fresh every
        // PrepareFrame keeps the behaviour in sync with no dependency on graph rebuild
        // timing (and no redundant field that can drift out of sync).
        [System.NonSerialized] public TimelineClip clip;

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ParticleSystemBehaviour>.Create(graph);
            ParticleSystemBehaviour behaviour = playable.GetBehaviour();
            behaviour.ParticleSystem = particleSystem.Resolve(graph.GetResolver());
            behaviour.HandleObjectActivation = handleObjectActivation;
            behaviour.Clip = clip;
            return playable;
        }
    }
}
