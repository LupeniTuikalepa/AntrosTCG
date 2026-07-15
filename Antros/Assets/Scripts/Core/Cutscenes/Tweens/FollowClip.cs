// Assets/Scripts/Core/Cutscenes/Tweens/FollowClip.cs
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Core.Cutscenes
{
    /// <summary>
    /// Makes the track-bound transform continuously follow another transform for the
    /// clip's duration (as opposed to GoToClip's one-shot tween from A to B). Position,
    /// rotation and scale each sync independently — leave one off to keep the bound
    /// transform's own value for that channel.
    ///
    /// Blending is handled entirely through FrameData.weight in FollowBehaviour rather
    /// than a custom track mixer: declaring ClipCaps.Blending lets clips on the same
    /// TweenTrack overlap with the usual Ease In/Out drag handles, and weight already
    /// reflects both a clip's own fade and any overlap with a neighbor — a fade-in eases
    /// the bound transform onto this clip's target, and two overlapping FollowClips (even
    /// targeting different transforms) cross-blend smoothly from one to the other.
    /// </summary>
    public class FollowClip : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("Transform to follow (resolved at runtime).")]
        public ExposedReference<Transform> target;

        [Space]
        public bool syncPosition = true;
        public bool syncRotation = true;
        public bool syncScale = true;

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<FollowBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            behaviour.Target = target.Resolve(graph.GetResolver());
            behaviour.SyncPosition = syncPosition;
            behaviour.SyncRotation = syncRotation;
            behaviour.SyncScale = syncScale;

            return playable;
        }
    }
}
