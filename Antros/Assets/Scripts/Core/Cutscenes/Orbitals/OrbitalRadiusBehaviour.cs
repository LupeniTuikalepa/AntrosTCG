// Assets/Scripts/Core/Cutscenes/OrbitalRadiusBehaviour.cs
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class OrbitalRadiusBehaviour : PlayableBehaviour
    {
        public float fromRadius;
        public float toRadius;
        public AnimationCurve ease;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is not CinemachineOrbitalFollow orbital)
                return;

            double duration = playable.GetDuration();
            double t = duration > 0d ? playable.GetTime() / duration : 0d;
            float k = ease != null ? ease.Evaluate((float)t) : (float)t;

            orbital.Radius = Mathf.Lerp(fromRadius, toRadius, k);
        }
    }
}