// Assets/Scripts/Core/Cutscenes/GoToBehaviour.cs
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class GoToBehaviour : PlayableBehaviour
    {
        public Transform from;
        public Transform to;
        public bool useLocalSpace;
        public AnimationCurve ease;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is not Transform target)
                return;
            if (to == null)
                return;

            double duration = playable.GetDuration();
            double t = duration > 0d ? playable.GetTime() / duration : 0d;
            float k = ease?.Evaluate((float)t) ?? (float)t;

            if (useLocalSpace)
            {
                Vector3 a = from != null ? from.localPosition : target.localPosition;
                Vector3 b = to.localPosition;
                target.localPosition = Vector3.LerpUnclamped(a, b, k);
            }
            else
            {
                Vector3 a = from != null ? from.position : target.position;
                Vector3 b = to.position;
                target.position = Vector3.LerpUnclamped(a, b, k);
            }
        }
    }
}