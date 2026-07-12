using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Loops
{
    /// <summary>
    /// Track holding loop clips. No binding: each clip points to its own component
    /// via ExposedReference. The track only feeds each clip its start/end times.
    /// </summary>
    [DisplayName("ATCG/Loop Track")]
    [TrackColor(0.5f, 0.3f, 0.8f)]
    [TrackClipType(typeof(LoopClip))]
    public class LoopTrack : TrackAsset
    {
        // Injects each clip's bounds before playables are created.
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset is LoopClip loopClip)
                {
                    loopClip.clipStart = clip.start;
                    loopClip.clipEnd = clip.end;
                }
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}