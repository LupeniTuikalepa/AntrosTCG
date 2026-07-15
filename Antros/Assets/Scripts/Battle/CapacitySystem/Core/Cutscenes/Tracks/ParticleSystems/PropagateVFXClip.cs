using ATCG.Battle.Entities.Runtime.VFX;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Points to its PropagateVFXOnRenderers via an ExposedReference (like ParticleSystemClip
    /// / CinemachineShot), so several clips can each drive a different propagator with no
    /// track binding. Ease In / Ease Out are read the same way as ParticleSystemClip: the
    /// window outside which the propagator stops emitting new particles on every instance
    /// it spawned, but lets whatever's already alive keep dying on its own.
    /// </summary>
    public sealed class PropagateVFXClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<PropagateVFXOnRenderers> propagator;

        // Injected by PropagateVFXTrack.CreateTrackMixer before CreatePlayable runs. Kept
        // as a live TimelineClip reference (not copied doubles) — see ParticleSystemClip.
        [System.NonSerialized] public TimelineClip clip;

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PropagateVFXBehaviour>.Create(graph);
            PropagateVFXBehaviour behaviour = playable.GetBehaviour();
            behaviour.Propagator = propagator.Resolve(graph.GetResolver());
            behaviour.Clip = clip;
            return playable;
        }
    }
}
