using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Track for particle-system clips referenced individually (no track binding — see
    /// ParticleSystemClip). Hands each clip its own live TimelineClip reference before its
    /// playable is created, so Ease In/Out stay in sync straight from the native handles.
    /// </summary>
    [DisplayName("ATCG/VFX/Particle System Track")]
    [TrackColor(0.9f, 0.55f, 0.15f)]
    [TrackClipType(typeof(ParticleSystemClip))]
    public sealed class ParticleSystemTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset is ParticleSystemClip particleClip)
                    particleClip.clip = clip;
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}
