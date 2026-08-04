using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Per-clip impact data, reusing Cinemachine's own impulse envelope (attack / sustain /
    /// decay with shape curves) but evaluated over the clip's own local time — so the impact
    /// is deterministic and frame-exact instead of a real-time one-shot. The envelope drives
    /// the referenced Perlin's amplitude/frequency (the random rattle) and, optionally, a
    /// directional kick pushed through a ScreenShakeImpactOffset on the camera.
    /// </summary>
    [Serializable]
    public sealed class ScreenShakeImpactBehaviour : PlayableBehaviour
    {
        public CinemachineImpulseManager.EnvelopeDefinition envelope = CinemachineImpulseManager.EnvelopeDefinition.Default;

        [Space]
        public float amplitudeGain = 1f;
        public float frequencyGain = 1f;

        [Space]
        [Tooltip("Directional kick (magnitude = strength). Zero = pure rattle, no push. Requires a ScreenShakeImpactOffset on the camera.")]
        public Vector3 direction = Vector3.zero;
        [Tooltip("Direction relative to the camera's facing (dynamic with the cutscene orientation) instead of world space.")]
        public bool directionInCameraSpace = true;

        [NonSerialized] public CinemachineBasicMultiChannelPerlin Target;

        // Envelope value in [0..1] at the given clip-local time (attack in, sustain, decay out).
        public float EvaluateEnvelope(double time) => envelope.GetValueAt((float)time);
    }
}
