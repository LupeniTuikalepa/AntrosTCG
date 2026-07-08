using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Timeline clip that sweeps a CinemachineSplineDolly along its spline. Authoring
    /// happens on the contained behaviour (from/to normalized + easing).
    /// </summary>
    [Serializable]
    public sealed class SplineDollyClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] private SplineDollyBehaviour template = new();

        public ClipCaps clipCaps => ClipCaps.Blending | ClipCaps.Extrapolation | ClipCaps.SpeedMultiplier;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
            => ScriptPlayable<SplineDollyBehaviour>.Create(graph, template);
    }
}
