// Assets/Scripts/Core/Cutscenes/OrbitalRotateBehaviour.cs
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class OrbitalRotateBehaviour : PlayableBehaviour
    {
        public float from;
        public float to;

        public AnimationCurve ease;


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is not CinemachineOrbitalFollow orbital)
                return;

            double duration = playable.GetDuration();
            double t = duration > 0d ? playable.GetTime() / duration : 0d;
            float k = ease?.Evaluate((float)t) ?? (float)t;

            orbital.HorizontalAxis.Value = Mathf.Lerp(from, to, k);
        }
    }
}