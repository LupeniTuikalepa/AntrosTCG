using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Per-clip impact data. The impact shape IS the clip's fades: the mixer scales amplitude
    /// by the clip weight, so fade-in = attack, the full-weight middle = sustain, fade-out =
    /// decay (edit them as the clip's Ease In/Out, or via the numeric fields in the clip
    /// inspector). Drives the referenced Perlin's amplitude/frequency for the shake.
    /// </summary>
    [Serializable]
    public sealed class ScreenShakeImpactBehaviour : PlayableBehaviour
    {
        [Tooltip("Noise profile driven during the impact. Leave empty to reuse the camera's own assigned profile.")]
        public NoiseSettings profile;
        public float amplitudeGain = 1f;
        public float frequencyGain = 1f;

        [NonSerialized] public CinemachineBasicMultiChannelPerlin Target;
    }
}
