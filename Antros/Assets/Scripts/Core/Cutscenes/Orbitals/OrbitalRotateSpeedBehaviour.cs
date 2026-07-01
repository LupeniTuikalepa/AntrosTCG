// Assets/Scripts/Core/Cutscenes/OrbitalRotateBehaviour.cs
using Unity.Cinemachine;
using UnityEngine.Playables;

namespace ATCG.Core.Cutscenes
{
    public class OrbitalSpeedRotateBehaviour : PlayableBehaviour
    {
        public float degreesPerSecond;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is not CinemachineOrbitalFollow orbital)
                return;

            // Rotation procédurale à vitesse constante, intégrée par deltaTime.
            orbital.HorizontalAxis.Value += degreesPerSecond * (float)info.deltaTime;
        }
    }
}