using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Timeline clip that plays a deterministic impact shake on the camera dragged onto it.
    /// Drop the target CinemachineBasicMultiChannelPerlin into the clip's inspector; the impact
    /// envelope (attack/sustain/decay, same as a Cinemachine impulse) shapes the Perlin's
    /// amplitude/frequency over the clip, and an optional Direction pushes the camera through a
    /// ScreenShakeImpactOffset. Size the clip to the envelope's duration. ClipCaps.None — the
    /// envelope is the shape, so there's nothing to ease.
    /// </summary>
    [Serializable]
    public sealed class ScreenShakeImpactClip : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("Perlin noise component of the camera to shake — glissez-le ici.")]
        public ExposedReference<CinemachineBasicMultiChannelPerlin> perlin;

        [SerializeField] private ScreenShakeImpactBehaviour template = new();

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ScreenShakeImpactBehaviour>.Create(graph, template);
            playable.GetBehaviour().Target = perlin.Resolve(graph.GetResolver());
            return playable;
        }
    }
}
