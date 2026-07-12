using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Track for PropagateVFXOnRenderers clips referenced individually (no track binding —
    /// see PropagateVFXClip). Injects each clip's Ease Out duration before its playable is
    /// created, same trick as ParticleSystemTrack/LoopTrack.
    /// </summary>
    [TrackColor(0.15f, 0.7f, 0.55f)]
    [TrackClipType(typeof(PropagateVFXClip))]
    public sealed class PropagateVFXTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset is PropagateVFXClip propagateClip)
                    propagateClip.easeOutDuration = clip.easeOutDuration;
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}
