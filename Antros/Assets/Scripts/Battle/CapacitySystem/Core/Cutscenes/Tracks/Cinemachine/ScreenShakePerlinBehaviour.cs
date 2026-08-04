using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Per-clip Perlin screen-shake data: the noise profile to play plus amplitude/frequency
    /// gains, and an intensity envelope evaluated over the clip (0..1 along its duration) that
    /// scales both amplitude and frequency — leave it flat for a steady shake, or shape it for an impact-then-
    /// settle punch. Clip weight (Ease In/Out and overlaps) is applied on top by the mixer.
    /// Target is the Perlin component dragged onto the clip, resolved from its ExposedReference
    /// in CreatePlayable (no track binding — each clip drives its own camera).
    /// </summary>
    [Serializable]
    public sealed class ScreenShakePerlinBehaviour : PlayableBehaviour
    {
        public NoiseSettings profile;
        public float amplitudeGain = 1f;
        public float frequencyGain = 1f;
        public AnimationCurve intensityOverTime = AnimationCurve.Constant(0f, 1f, 1f);

        [NonSerialized] public CinemachineBasicMultiChannelPerlin Target;

        // Envelope multiplier for the given clip-local [0,1] progress.
        public float EvaluateIntensity(double progress)
            => intensityOverTime != null ? intensityOverTime.Evaluate((float)progress) : 1f;
    }
}
