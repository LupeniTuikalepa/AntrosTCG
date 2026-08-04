using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Timeline clip that plays a Cinemachine Perlin screen shake on the camera dragged onto
    /// it. Drop the target CinemachineBasicMultiChannelPerlin into the clip's inspector (the
    /// Perlin field) — no track binding, so different clips on the same Screen Shake Track can
    /// each drive a different camera. Authoring (noise profile, amplitude/frequency, intensity
    /// envelope) lives on the contained behaviour; the track mixer blends every active clip
    /// onto its target and previews deterministically while scrubbing.
    /// </summary>
    [Serializable]
    public sealed class ScreenShakePerlinClip : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("Perlin noise component of the camera to shake — glissez-le ici.")]
        public ExposedReference<CinemachineBasicMultiChannelPerlin> perlin;

        [SerializeField] private ScreenShakePerlinBehaviour template = new();

        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Extrapolation | ClipCaps.SpeedMultiplier;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ScreenShakePerlinBehaviour>.Create(graph, template);
            playable.GetBehaviour().Target = perlin.Resolve(graph.GetResolver());
            return playable;
        }
    }
}
