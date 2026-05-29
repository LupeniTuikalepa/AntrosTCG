using UnityEngine;

namespace Helteix.Tools.Phases.Utilities
{
    public static class PhaseUtilities
    {
        public static async Awaitable WaitFor<T>(this T phase) where T : IPhase => await new WaitForPhase<T>(phase).Wait();

        public static async Awaitable WaitForAny<T>() where T : IPhase => await new WaitForAnyPhase<T>().Wait();

        public static async Awaitable WaitForResult<TValue>(this Phase<TValue> phase) => await new WaitForPhaseResult<TValue>(phase).WaitForValue();

    }
}