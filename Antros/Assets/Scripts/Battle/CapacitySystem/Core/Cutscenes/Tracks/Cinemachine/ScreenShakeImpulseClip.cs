using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Timeline clip that fires one Cinemachine impulse. Authoring lives on the contained
    /// behaviour (impulse profile + velocity). ClipCaps.None: an impulse is a one-shot event,
    /// so there is nothing to blend — the clip just marks when it fires.
    /// </summary>
    [Serializable]
    public sealed class ScreenShakeImpulseClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeField] private ScreenShakeImpulseBehaviour template = new();

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
            => ScriptPlayable<ScreenShakeImpulseBehaviour>.Create(graph, template);
    }
}
