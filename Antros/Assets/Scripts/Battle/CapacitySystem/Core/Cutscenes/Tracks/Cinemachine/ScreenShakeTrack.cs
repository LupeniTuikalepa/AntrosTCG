using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Cinemachine screen-shake track. Each impact clip drives the CinemachineBasicMultiChannelPerlin
    /// dragged onto it, shaped by the clip's fades (fade-in = attack, middle = sustain, fade-out =
    /// decay) — deterministic, previews while scrubbing, and overlapping clips add. No track
    /// binding: clips carry their own camera reference.
    /// </summary>
    [DisplayName("ATCG/Cinemachine/Screen Shake Track")]
    [TrackColor(0.9f, 0.4f, 0.25f)]
    [TrackClipType(typeof(ScreenShakeImpactClip))]
    public sealed class ScreenShakeTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
            => ScriptPlayable<ScreenShakeMixerBehaviour>.Create(graph, inputCount);
    }
}
