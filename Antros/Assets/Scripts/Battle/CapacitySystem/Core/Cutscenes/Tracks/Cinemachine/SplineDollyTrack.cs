using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Timeline track that drives a CinemachineSplineDolly's position along its spline.
    /// Bind the track to the dolly component; each clip sweeps normalized from→to over
    /// its duration, so a default 0→1 clip walks the whole spline end to end.
    /// </summary>
    [TrackColor(0.2f, 0.65f, 0.9f)]
    [TrackClipType(typeof(SplineDollyClip))]
    [TrackBindingType(typeof(CinemachineSplineDolly))]
    public sealed class SplineDollyTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
            => ScriptPlayable<SplineDollyMixerBehaviour>.Create(graph, inputCount);
    }
}
