using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Single Cinemachine screen-shake track hosting both shake models, mixed by one mixer:
    ///  - Perlin clips drive the CinemachineBasicMultiChannelPerlin dragged onto each clip,
    ///    blended by clip weight (Ease In/Out, overlaps add), with deterministic editor
    ///    preview — use for a shake you want to see while scrubbing and fade in/out.
    ///  - Impact clips shape a dragged camera's Perlin with a Cinemachine attack/sustain/decay
    ///    envelope over the clip's own time (deterministic, previews), plus an optional
    ///    directional kick — use for punchy hits.
    ///  - Impulse clips fire a one-shot CinemachineImpulseDefinition that every camera with a
    ///    CinemachineImpulseListener reacts to (Play mode only) — use for a global hit.
    /// No track binding: Perlin/Impact clips carry their own camera reference, impulses are global.
    /// </summary>
    [DisplayName("ATCG/Cinemachine/Screen Shake Track")]
    [TrackColor(0.9f, 0.4f, 0.25f)]
    [TrackClipType(typeof(ScreenShakePerlinClip))]
    [TrackClipType(typeof(ScreenShakeImpactClip))]
    [TrackClipType(typeof(ScreenShakeImpulseClip))]
    public sealed class ScreenShakeTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
            => ScriptPlayable<ScreenShakeMixerBehaviour>.Create(graph, inputCount);
    }
}
