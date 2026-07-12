using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Points to its ParticleSystem via an ExposedReference (like CinemachineShot), so
    /// several clips can each target a different system with no track binding. Ease Out
    /// (the native Timeline clip handle — draggable in the timeline and editable as a
    /// plain number in the clip Inspector) is read as the "shutdown" length: emission
    /// stops there while already-alive particles keep dying naturally, and the system is
    /// fully stopped and deactivated only once the clip is actually exited.
    /// </summary>
    public sealed class ParticleSystemClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<ParticleSystem> particleSystem;

        // Injected by ParticleSystemTrack.CreateTrackMixer before CreatePlayable runs —
        // mirrors LoopClip's clipStart/clipEnd. Not authored here directly: the native
        // Ease Out handle already gives the value, the visual, and an Inspector field.
        public double easeOutDuration;

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ParticleSystemBehaviour>.Create(graph);
            ParticleSystemBehaviour behaviour = playable.GetBehaviour();
            behaviour.ParticleSystem = particleSystem.Resolve(graph.GetResolver());
            behaviour.EaseOutDuration = easeOutDuration;
            return playable;
        }
    }
}
