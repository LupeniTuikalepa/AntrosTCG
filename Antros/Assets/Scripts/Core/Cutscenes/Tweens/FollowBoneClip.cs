// Assets/Scripts/Core/Cutscenes/Tweens/FollowBoneClip.cs
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Core.Cutscenes
{
    /// <summary>
    /// Makes the track-bound transform follow a bone of the caster's Animator for the
    /// clip's duration. The caster is not referenced here: it is resolved at runtime through
    /// the cutscene injection system via ICutsceneCasterAnimatorSource (a Battle element that
    /// receives the injected CASTER and exposes its Animator), found on the director owner —
    /// so the same clip works in game and in the editor preview with no serialized reference.
    ///
    /// Position and rotation sync independently (leave one off to keep the bound transform's
    /// own value for that channel); each has an offset applied in the bone's local space so
    /// it rides along as the bone moves. Blending mirrors FollowClip: ClipCaps.Blending plus
    /// FrameData.weight in the behaviour, so this clip overlaps and cross-fades with any other
    /// TweenTrack clip through the usual Ease In/Out handles — no custom track mixer.
    /// </summary>
    public class FollowBoneClip : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("Bone humanoïde à suivre. Ignoré si un nom est renseigné ci-dessous.")]
        public HumanBodyBones bone = HumanBodyBones.RightHand;

        [Tooltip("Optionnel : suit le bone portant ce nom exact (rig générique / point d'attache). Prioritaire sur le bone humanoïde.")]
        public string boneNameOverride = "";

        [Space]
        [Tooltip("Suit la position du bone (+ offset ci-dessous).")]
        public bool syncPosition = true;
        [Tooltip("Offset de position, en espace local du bone.")]
        public Vector3 positionOffset;

        [Space]
        [Tooltip("Suit la rotation du bone (+ offset ci-dessous).")]
        public bool syncRotation = true;
        [Tooltip("Offset de rotation (Euler), appliqué après la rotation du bone.")]
        public Vector3 rotationOffset;

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<FollowBoneBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            // The caster source is an injected element somewhere on the cutscene rig; search
            // the whole hierarchy from the owner so it's found wherever the author placed it.
            behaviour.Source = owner != null
                ? owner.transform.root.GetComponentInChildren<ICutsceneCasterAnimatorSource>(true)
                : null;
            behaviour.Bone = bone;
            behaviour.BoneNameOverride = boneNameOverride;
            behaviour.SyncPosition = syncPosition;
            behaviour.SyncRotation = syncRotation;
            behaviour.PositionOffset = positionOffset;
            behaviour.RotationOffset = rotationOffset;

            return playable;
        }
    }
}
