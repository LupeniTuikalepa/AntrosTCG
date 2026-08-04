using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Mixer for the single Screen Shake Track. Handles the clip kinds in one pass:
    ///  - Perlin and Impact clips accumulate a weighted shake onto their referenced Perlin
    ///    component (grouped per target so several clips blend onto the same camera, or drive
    ///    different cameras). Amplitude is a weighted sum, frequency a weighted average, and the
    ///    loudest Perlin clip's profile wins on a crossfade (Impact clips keep the camera's own
    ///    authored profile). Each touched component's authored values are cached and restored
    ///    once it stops being driven or the graph is torn down.
    ///  - Impact clips additionally push a directional offset through a ScreenShakeImpactOffset
    ///    on the target camera (summed per extension, zeroed when idle).
    ///  - Impulse clips fire once on the rising edge where they become active (Play mode only;
    ///    the impulse manager doesn't tick while scrubbing).
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

        private readonly Dictionary<CinemachineBasicMultiChannelPerlin, ScreenShakeImpactOffset> offsetExtCache = new();
        private readonly Dictionary<ScreenShakeImpactOffset, Vector3> offsetAccum = new();
        private readonly HashSet<ScreenShakeImpactOffset> offsetDrivenLastFrame = new();
        private readonly HashSet<ScreenShakeImpactOffset> offsetDrivenThisFrame = new();

        private bool[] impulseWasActive;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            int count = playable.GetInputCount();
            if (impulseWasActive == null || impulseWasActive.Length != count)
                impulseWasActive = new bool[count];

            accum.Clear();
            drivenThisFrame.Clear();
            offsetAccum.Clear();
            offsetDrivenThisFrame.Clear();

            for (int i = 0; i < count; i++)
            {
                Playable input = playable.GetInput(i);
                float weight = playable.GetInputWeight(i);
                System.Type type = input.GetPlayableType();

                if (type == typeof(ScreenShakePerlinBehaviour))
                {
                    if (weight <= 0f)
                        continue;

                    var perlinInput = (ScriptPlayable<ScreenShakePerlinBehaviour>)input;
                    ScreenShakePerlinBehaviour data = perlinInput.GetBehaviour();
                    CinemachineBasicMultiChannelPerlin target = data.Target;
                    if (target == null)
                        continue;

                    double duration = perlinInput.GetDuration();
                    double time = perlinInput.GetTime();
                    double progress = duration > 0.0 ? time / duration : 0.0;
                    float intensity = data.EvaluateIntensity(progress);

                    Accumulate(target, data.amplitudeGain * intensity * weight, data.frequencyGain * intensity * weight, weight, data.profile);
                }
                else if (type == typeof(ScreenShakeImpactBehaviour))
                {
                    if (weight <= 0f)
                        continue;

                    var impactInput = (ScriptPlayable<ScreenShakeImpactBehaviour>)input;
                    ScreenShakeImpactBehaviour data = impactInput.GetBehaviour();
                    CinemachineBasicMultiChannelPerlin target = data.Target;
                    if (target == null)
                        continue;

                    float env = data.EvaluateEnvelope(impactInput.GetTime());

                    // Impact keeps the camera's own authored noise profile (null dominant).
                    Accumulate(target, data.amplitudeGain * env * weight, data.frequencyGain * env * weight, weight, null);

                    if (data.direction != Vector3.zero)
                        AccumulateOffset(target, data, env * weight);
                }
                else if (type == typeof(ScreenShakeImpulseBehaviour))
                {
                    bool active = weight > 0f;
                    if (Application.isPlaying && active && !impulseWasActive[i])
                    {
                        var impulseInput = (ScriptPlayable<ScreenShakeImpulseBehaviour>)input;
                        ScreenShakeImpulseBehaviour data = impulseInput.GetBehaviour();
                        data.definition?.CreateEvent(Vector3.zero, data.velocity);
                    }
                    impulseWasActive[i] = active;
                }
            }

            WritePerlins();
            WriteOffsets();
        }

        // Graph rebuild / director stop: put every touched component and offset back to rest.
        public override void OnPlayableDestroy(Playable playable)
        {
            foreach (KeyValuePair<CinemachineBasicMultiChannelPerlin, Defaults> pair in defaults)
                if (pair.Key != null)
                    RestoreDefaults(pair.Key);

            foreach (ScreenShakeImpactOffset ext in offsetExtCache.Values)
                if (ext != null)
                    ext.Offset = Vector3.zero;

            defaults.Clear();
            drivenLastFrame.Clear();
            offsetExtCache.Clear();
            offsetDrivenLastFrame.Clear();
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

        private void AccumulateOffset(CinemachineBasicMultiChannelPerlin target, ScreenShakeImpactBehaviour data, float scale)
        {
            ScreenShakeImpactOffset ext = ResolveOffset(target);
            if (ext == null)
                return;

            Vector3 world = data.directionInCameraSpace ? target.transform.rotation * data.direction : data.direction;
            offsetAccum.TryGetValue(ext, out Vector3 current);
            offsetAccum[ext] = current + world * scale;
            offsetDrivenThisFrame.Add(ext);
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

        private void WriteOffsets()
        {
            foreach (KeyValuePair<ScreenShakeImpactOffset, Vector3> pair in offsetAccum)
                if (pair.Key != null)
                    pair.Key.Offset = pair.Value;

            foreach (ScreenShakeImpactOffset ext in offsetDrivenLastFrame)
                if (ext != null && !offsetDrivenThisFrame.Contains(ext))
                    ext.Offset = Vector3.zero;

            offsetDrivenLastFrame.Clear();
            foreach (ScreenShakeImpactOffset ext in offsetDrivenThisFrame)
                offsetDrivenLastFrame.Add(ext);
        }

        private ScreenShakeImpactOffset ResolveOffset(CinemachineBasicMultiChannelPerlin target)
        {
            if (offsetExtCache.TryGetValue(target, out ScreenShakeImpactOffset ext))
                return ext;

            target.TryGetComponent(out ext);
            offsetExtCache[target] = ext;
            return ext;
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
