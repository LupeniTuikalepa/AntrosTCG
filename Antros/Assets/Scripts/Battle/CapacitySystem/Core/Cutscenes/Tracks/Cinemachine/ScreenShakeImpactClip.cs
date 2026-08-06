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
    /// shape IS the clip's fades — fade-in = attack, middle = sustain, fade-out = decay — with
    /// the mixer scaling the Perlin amplitude by the clip weight. ClipCaps.Blending so the Ease
    /// In/Out handles exist and overlapping impacts add.
    /// </summary>
    [Serializable]
    public sealed class ScreenShakeImpactClip : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("Perlin noise component of the camera to shake — glissez-le ici.")]
        public ExposedReference<CinemachineBasicMultiChannelPerlin> perlin;

        [SerializeField] private ScreenShakeImpactBehaviour template = new();

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ScreenShakeImpactBehaviour>.Create(graph, template);
            playable.GetBehaviour().Target = perlin.Resolve(graph.GetResolver());
            return playable;
        }
    }
}
