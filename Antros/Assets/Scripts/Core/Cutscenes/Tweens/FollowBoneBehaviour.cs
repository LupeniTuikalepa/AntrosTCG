// Assets/Scripts/Core/Cutscenes/Tweens/FollowBoneBehaviour.cs
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class FollowBoneBehaviour : PlayableBehaviour
    {
        public ICutsceneCasterAnimatorSource Source;
        public HumanBodyBones Bone;
        public string BoneNameOverride;
        public bool SyncPosition;
        public bool SyncRotation;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;

        private Transform resolvedBone;
        private Animator resolvedFrom;

        // Same weight-driven hand-off as FollowBehaviour: weight 1 snaps the bound transform
        // onto the bone (live follow), a fading weight (Ease In/Out, or an overlapping
        // neighbour) eases it there, so this clip cross-blends with any other TweenTrack clip
        // through FrameData.weight alone. Offsets ride in the bone's own space so they stay
        // attached as the bone rotates.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is not Transform bound)
                return;

            Transform target = ResolveBone();
            if (target == null)
                return;

            float weight = info.weight;
            if (weight <= 0f)
                return;

            if (SyncPosition)
            {
                Vector3 position = target.position + target.rotation * PositionOffset;
                bound.position = Vector3.LerpUnclamped(bound.position, position, weight);
            }

            if (SyncRotation)
            {
                Quaternion rotation = target.rotation * Quaternion.Euler(RotationOffset);
                bound.rotation = Quaternion.SlerpUnclamped(bound.rotation, rotation, weight);
            }
        }

        // The caster Animator is injected on Connect, which can land before or after the graph
        // is built, so resolve the bone lazily; re-resolve when the Animator instance changes
        // (recompile, or a re-selected preview actor).
        private Transform ResolveBone()
        {
            Animator animator = Source?.CasterAnimator;
            if (animator == null)
            {
                resolvedBone = null;
                resolvedFrom = null;
                return null;
            }

            if (resolvedBone != null && ReferenceEquals(animator, resolvedFrom))
                return resolvedBone;

            resolvedFrom = animator;
            resolvedBone = !string.IsNullOrEmpty(BoneNameOverride)
                ? FindByName(animator.transform, BoneNameOverride)
                : animator.isHuman ? animator.GetBoneTransform(Bone) : null;

            return resolvedBone;
        }

        // Depth-first search by exact name under the animator root.
        private static Transform FindByName(Transform root, string name)
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
