using UnityEngine;

namespace Helteix.Tools.Phases.Utilities
{
    internal class WaitForPhase<T> : IPhaseListener<T> where T : IPhase
    {
        protected readonly T current;
        private AwaitableCompletionSource completionSource;

        public WaitForPhase(T current)
        {
            this.current = current;
        }

        void IPhaseListener<T>.OnPhaseBegin(T phase) { }

        void IPhaseListener<T>.OnPhaseEnd(T phase)
        {
            if (Equals(phase, current))
                completionSource.SetResult();
        }

        internal virtual async Awaitable Wait()
        {
            // If already done before we even start, return immediately.
            if (!current.IsRunning())
                return;

            completionSource = new AwaitableCompletionSource();
            PhaseManager.Register(this);

            await completionSource.Awaitable;

            PhaseManager.Unregister(this);
        }
    }
}