using Unity.Cinemachine;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Cinemachine extension that adds a world-space positional kick to the camera after the
    /// pipeline (via the same Correction channel as noise, so it composes with a Perlin shake).
    /// Screen-shake impact clips drive Offset each frame from their envelope; it sits at zero
    /// the rest of the time. Add this to any camera that should receive directional impacts —
    /// leave it off and impact clips still produce their Perlin rattle, just no push.
    /// </summary>
    [AddComponentMenu("ATCG/Cinemachine/Screen Shake Impact Offset")]
    public sealed class ScreenShakeImpactOffset : CinemachineExtension
    {
        // World-space additive offset, written by the track mixer; reset to zero when no impact
        // clip is driving it.
        public Vector3 Offset;

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage,
            ref CameraState state, float deltaTime)
        {
            if (stage == CinemachineCore.Stage.Finalize && Offset != Vector3.zero)
                state.PositionCorrection += Offset;
        }
    }
}
