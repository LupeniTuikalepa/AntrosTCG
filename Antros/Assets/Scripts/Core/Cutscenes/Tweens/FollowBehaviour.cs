// Assets/Scripts/Core/Cutscenes/Tweens/FollowBehaviour.cs
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class FollowBehaviour : PlayableBehaviour
    {
        public Transform Target;
        public bool SyncPosition;
        public bool SyncRotation;
        public bool SyncScale;

        // Blends this clip's contribution onto whatever value the bound transform
        // already has this frame — weight 1 (fully active, no overlap) snaps straight
        // onto Target every frame, which is exactly a live follow; a fading weight (Ease
        // In/Out, or an overlapping neighbor clip fading out) eases the bound transform
        // toward Target instead of cutting to it, giving the smooth hand-off between two
        // overlapping FollowClips asked for.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is not Transform bound)
                return;
            if (Target == null)
                return;

            float weight = info.weight;
            if (weight <= 0f)
                return;

            if (SyncPosition)
                bound.position = Vector3.LerpUnclamped(bound.position, Target.position, weight);

            if (SyncRotation)
                bound.rotation = Quaternion.SlerpUnclamped(bound.rotation, Target.rotation, weight);

            if (SyncScale)
                bound.localScale = Vector3.LerpUnclamped(bound.localScale, Target.localScale, weight);
        }
    }
}
