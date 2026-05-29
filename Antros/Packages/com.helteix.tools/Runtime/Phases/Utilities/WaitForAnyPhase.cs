using UnityEngine;

namespace Helteix.Tools.Phases.Utilities
{
    internal class WaitForAnyPhase<T> : IPhaseListener<T> where T : IPhase
    {
        private AwaitableCompletionSource completionSource;

        void IPhaseListener<T>.OnPhaseBegin(T phase) { }

        void IPhaseListener<T>.OnPhaseEnd(T phase)
        {
            completionSource.SetResult();
        }

        public async Awaitable Wait()
        {
            completionSource = new AwaitableCompletionSource();
            PhaseManager.Register(this);

            await completionSource.Awaitable;

            PhaseManager.Unregister(this);
        }
    }
}