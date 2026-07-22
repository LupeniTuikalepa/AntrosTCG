// Assets/Scripts/Core/Cutscenes/Tweens/FollowBoneClip.cs
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ATCG.Core.Cutscenes
{
    /// <summary>
    /// Makes the track-bound transform follow a bone of the caster's Animator for the
    /// clip's duration. Position and rotation sync independently (leave one off to keep
    /// the bound transform's own value for that channel); each has an offset applied in
    /// the bone's local space so it rides along as the bone moves.
    ///
    /// Blending mirrors FollowClip: ClipCaps.Blending plus FrameData.weight inside the
    /// behaviour, so this clip overlaps and cross-fades with any other TweenTrack clip
    /// through the usual Ease In/Out handles — no custom track mixer.
    /// </summary>
    public class FollowBoneClip : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("Animator du caster (résolu à l'exécution).")]
        public ExposedReference<Animator> caster;

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

            Animator animator = caster.Resolve(graph.GetResolver());
            behaviour.Bone = ResolveBone(animator);
            behaviour.SyncPosition = syncPosition;
            behaviour.SyncRotation = syncRotation;
            behaviour.PositionOffset = positionOffset;
            behaviour.RotationOffset = rotationOffset;

            return playable;
        }

        // Name override wins (handles generic rigs and non-humanoid mount points);
        // otherwise fall back to the humanoid bone map when the rig is humanoid.
        Transform ResolveBone(Animator animator)
        {
            if (animator == null)
                return null;

            if (!string.IsNullOrEmpty(boneNameOverride))
                return FindByName(animator.transform, boneNameOverride);

            return animator.isHuman ? animator.GetBoneTransform(bone) : null;
        }

        // Depth-first search by exact name under the animator root.
        static Transform FindByName(Transform root, string name)
        {
            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindByName(root.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
