using System;
using UnityEngine;
using UnityEngine.Playables;

namespace ATCG.Battle.CapacitySystem.Core.Cutscenes.Tracks
{
    /// <summary>
    /// Per-clip data for a spline dolly move: normalized start/end positions along the
    /// spline (0 = spline start, 1 = spline end) plus an easing curve. Normalized values
    /// keep clips unit-agnostic; the mixer converts to the dolly's actual PositionUnits.
    /// </summary>
    [Serializable]
    public sealed class SplineDollyBehaviour : PlayableBehaviour
    {
        [Range(0f, 1f)] public float from = 0f;
        [Range(0f, 1f)] public float to = 1f;
        public AnimationCurve easing = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        // Eased, clip-local normalized position for the given [0,1] progress.
        public float Evaluate(double progress)
        {
            float t = easing != null ? easing.Evaluate((float)progress) : (float)progress;
            return Mathf.LerpUnclamped(from, to, t);
        }
    }
}
