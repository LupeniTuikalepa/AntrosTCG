// Assets/Scripts/Core/Cutscenes/Tweens/FollowBoneBehaviour.cs
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class FollowBoneBehaviour : PlayableBehaviour
    {
        public Transform Bone;
        public bool SyncPosition;
        public bool SyncRotation;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;

        // Same weight-driven hand-off as FollowBehaviour: weight 1 (fully active, no
        // overlap) snaps the bound transform onto the bone every frame (a live follow);
        // a fading weight (Ease In/Out, or an overlapping neighbour fading out) eases it
        // toward the bone instead of cutting, so this clip cross-blends with any other
        // TweenTrack clip through FrameData.weight alone. Offsets ride in the bone's own
        // space so they stay attached as the bone rotates (a muzzle/hand mount point
        // rather than a fixed world nudge).
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is not Transform bound)
                return;
            if (Bone == null)
                return;

            float weight = info.weight;
            if (weight <= 0f)
                return;

            if (SyncPosition)
            {
                Vector3 target = Bone.position + Bone.rotation * PositionOffset;
                bound.position = Vector3.LerpUnclamped(bound.position, target, weight);
            }

            if (SyncRotation)
            {
                Quaternion target = Bone.rotation * Quaternion.Euler(RotationOffset);
                bound.rotation = Quaternion.SlerpUnclamped(bound.rotation, target, weight);
            }
        }
    }
}
