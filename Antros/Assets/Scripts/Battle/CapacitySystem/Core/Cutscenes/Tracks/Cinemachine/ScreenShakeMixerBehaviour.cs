using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Mixer for the Screen Shake Track. Each impact clip accumulates a weighted shake onto its
    /// referenced Perlin, grouped per target so several clips blend onto the same camera or drive
    /// different ones. The clip weight IS the envelope (fade-in = attack, middle = sustain,
    /// fade-out = decay): amplitude is a weighted sum, frequency a weighted average, and the
    /// loudest clip's profile wins on a crossfade (else the camera's own profile). Each touched
    /// component's authored values are cached and restored once it stops being driven or the
    /// graph is torn down, so the camera settles back cleanly.
    /// </summary>
    public sealed class ScreenShakeMixerBehaviour : PlayableBehaviour
    {
        private struct Accum
        {
            public float amplitude;
            public float frequencyAccum;
            public float totalWeight;
            public float dominantWeight;
            public NoiseSettings dominantProfile;
        }

        private struct Defaults
        {
            public NoiseSettings profile;
            public float amplitude;
            public float frequency;
        }

        private readonly Dictionary<CinemachineBasicMultiChannelPerlin, Accum> accum = new();
        private readonly Dictionary<CinemachineBasicMultiChannelPerlin, Defaults> defaults = new();
        private readonly HashSet<CinemachineBasicMultiChannelPerlin> drivenLastFrame = new();
        private readonly HashSet<CinemachineBasicMultiChannelPerlin> drivenThisFrame = new();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            accum.Clear();
            drivenThisFrame.Clear();

            int count = playable.GetInputCount();
            for (int i = 0; i < count; i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight <= 0f)
                    continue;

                var input = (ScriptPlayable<ScreenShakeImpactBehaviour>)playable.GetInput(i);
                ScreenShakeImpactBehaviour data = input.GetBehaviour();
                CinemachineBasicMultiChannelPerlin target = data.Target;
                if (target == null)
                    continue;

                // The clip weight IS the impact envelope. Impact drives its own profile if set,
                // else falls back (null) to the camera's authored profile — without a profile the
                // Perlin is invalid and renders nothing.
                Accumulate(target, data.amplitudeGain * weight, data.frequencyGain * weight, weight, data.profile);
            }

            WritePerlins();
        }

        // Graph rebuild / director stop: put every touched component back to rest.
        public override void OnPlayableDestroy(Playable playable)
        {
            foreach (KeyValuePair<CinemachineBasicMultiChannelPerlin, Defaults> pair in defaults)
                if (pair.Key != null)
                    RestoreDefaults(pair.Key);

            defaults.Clear();
            drivenLastFrame.Clear();
        }

        private void Accumulate(CinemachineBasicMultiChannelPerlin target, float amplitude, float frequency, float weight, NoiseSettings profile)
        {
            CacheDefaults(target);
            drivenThisFrame.Add(target);

            accum.TryGetValue(target, out Accum a);
            a.amplitude += amplitude;
            a.frequencyAccum += frequency;
            a.totalWeight += weight;
            if (profile != null && weight > a.dominantWeight)
            {
                a.dominantWeight = weight;
                a.dominantProfile = profile;
            }
            accum[target] = a;
        }

        private void WritePerlins()
        {
            foreach (KeyValuePair<CinemachineBasicMultiChannelPerlin, Accum> pair in accum)
            {
                CinemachineBasicMultiChannelPerlin target = pair.Key;
                Accum a = pair.Value;
                if (a.totalWeight <= 0f)
                    continue;

                target.NoiseProfile = a.dominantProfile != null ? a.dominantProfile : defaults[target].profile;
                target.AmplitudeGain = a.amplitude;
                target.FrequencyGain = a.frequencyAccum / a.totalWeight;
            }

            foreach (CinemachineBasicMultiChannelPerlin target in drivenLastFrame)
                if (target != null && !drivenThisFrame.Contains(target))
                    RestoreDefaults(target);

            drivenLastFrame.Clear();
            foreach (CinemachineBasicMultiChannelPerlin target in drivenThisFrame)
                drivenLastFrame.Add(target);
        }

        private void CacheDefaults(CinemachineBasicMultiChannelPerlin perlin)
        {
            if (defaults.ContainsKey(perlin))
                return;

            defaults[perlin] = new Defaults
            {
                profile = perlin.NoiseProfile,
                amplitude = perlin.AmplitudeGain,
                frequency = perlin.FrequencyGain
            };
        }

        private void RestoreDefaults(CinemachineBasicMultiChannelPerlin perlin)
        {
            if (!defaults.TryGetValue(perlin, out Defaults d))
                return;

            perlin.NoiseProfile = d.profile;
            perlin.AmplitudeGain = d.amplitude;
            perlin.FrequencyGain = d.frequency;
        }
    }
}
