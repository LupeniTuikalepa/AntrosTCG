using System.Collections.Generic;
using Helteix.Tools;
using UnityEngine;

namespace ATCG.Cutscenes
{
    /// <summary>
    /// Per-step synchronisation across screens. A step runs only once EVERY screen
    /// has reported its StepMarker (same idea as CommandListenerState wind-up).
    /// Online: one screen -> releases on first report. Local multi-screen: waits for
    /// the slowest. Reports may arrive in any order and before/after arming, so the
    /// barrier counts reports per step name and releases when the count reaches the
    /// screen count.
    /// </summary>
    public sealed class StepBarrier
    {
        private readonly int screenCount;
        private readonly float timeout;
        private readonly Dictionary<string, int> reportsByStep = new();

        private string armedStep;
        private AwaitableCompletionSource armedSource;

        public StepBarrier(int screenCount, float timeout = 5f)
        {
            this.screenCount = Mathf.Max(1, screenCount);
            this.timeout = timeout;
        }

        /// <summary>
        /// Wait until all screens have reported <paramref name="stepName"/>. Safe to
        /// call after some reports already arrived (count is preserved per step).
        /// </summary>
        public Awaitable Await(string stepName)
        {
            armedStep = stepName;
            armedSource = new AwaitableCompletionSource();

            reportsByStep.TryGetValue(stepName, out int already);
            if (already >= screenCount)
                armedSource.TrySetResult();
            else
                WatchTimeout(stepName, armedSource).ListenForExceptions();

            return armedSource.Awaitable;
        }

        /// <summary>A screen reports it reached the marker for <paramref name="stepName"/>.</summary>
        public void Report(string stepName)
        {
            reportsByStep.TryGetValue(stepName, out int count);
            count++;
            reportsByStep[stepName] = count;

            if (stepName == armedStep && armedSource != null && count >= screenCount)
                armedSource.TrySetResult();
        }

        private async Awaitable WatchTimeout(string stepName, AwaitableCompletionSource src)
        {
            await Awaitable.WaitForSecondsAsync(timeout);

            if (src == armedSource && !IsComplete(stepName) && src.TrySetResult())
            {
                Debug.LogError(
                    $"[StepBarrier] Timeout: step '{stepName}' incomplete after {timeout}s " +
                    $"({Reports(stepName)}/{screenCount}). Online: desync -> disconnect. Local: bug.");
            }
        }

        private bool IsComplete(string stepName)
        {
            reportsByStep.TryGetValue(stepName, out int c);
            return c >= screenCount;
        }

        private int Reports(string stepName)
        {
            reportsByStep.TryGetValue(stepName, out int c);
            return c;
        }
    }
}