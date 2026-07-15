using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Track for PropagateVFXOnRenderers clips referenced individually (no track binding —
    /// see PropagateVFXClip). Hands each clip its own live TimelineClip reference before its
    /// playable is created, same trick as ParticleSystemTrack.
    /// </summary>
    [DisplayName("ATCG/VFX/Propagate VFX Track")]
    [TrackColor(0.15f, 0.7f, 0.55f)]
    [TrackClipType(typeof(PropagateVFXClip))]
    public sealed class PropagateVFXTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset is PropagateVFXClip propagateClip)
                    propagateClip.clip = clip;
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}
