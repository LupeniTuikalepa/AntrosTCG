using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Splines;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Blends the active spline-dolly clips and writes the resulting position onto the
    /// bound CinemachineSplineDolly. Clips author normalized [0,1] positions; this mixer
    /// converts to the dolly's current PositionUnits so "0..1" always means end-to-end
    /// regardless of whether the component uses Normalized, Distance, or Knot units.
    /// </summary>
    public sealed class SplineDollyMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is not CinemachineSplineDolly dolly)
                return;

            int count = playable.GetInputCount();
            float normalized = 0f;
            float totalWeight = 0f;

            for (int i = 0; i < count; i++)
            {
                float weight = playable.GetInputWeight(i);
                if (weight <= 0f)
                    continue;

                ScriptPlayable<SplineDollyBehaviour> input = (ScriptPlayable<SplineDollyBehaviour>)playable.GetInput(i);
                SplineDollyBehaviour data = input.GetBehaviour();

                double duration = input.GetDuration();
                double time = input.GetTime();
                double progress = duration > 0.0 ? time / duration : 0.0;

                normalized += data.Evaluate(progress) * weight;
                totalWeight += weight;
            }

            if (totalWeight <= 0f)
                return;

            normalized /= totalWeight;
            dolly.CameraPosition = ToUnits(dolly, Mathf.Clamp01(normalized));
        }

        // Converts a normalized [0,1] spline position to the dolly's active PositionUnits.
        private static float ToUnits(CinemachineSplineDolly dolly, float normalized)
        {
            switch (dolly.PositionUnits)
            {
                case PathIndexUnit.Normalized:
                    return normalized;

                case PathIndexUnit.Distance:
                    return normalized * SplineLength(dolly);

                case PathIndexUnit.Knot:
                    int knots = KnotCount(dolly);
                    return knots > 1 ? normalized * (knots - 1) : 0f;

                default:
                    return normalized;
            }
        }

        private static float SplineLength(CinemachineSplineDolly dolly)
        {
            SplineContainer container = dolly.Spline;
            return container != null ? container.CalculateLength() : 0f;
        }

        private static int KnotCount(CinemachineSplineDolly dolly)
        {
            SplineContainer container = dolly.Spline;
            return container != null && container.Spline != null ? container.Spline.Count : 0;
        }
    }
}
