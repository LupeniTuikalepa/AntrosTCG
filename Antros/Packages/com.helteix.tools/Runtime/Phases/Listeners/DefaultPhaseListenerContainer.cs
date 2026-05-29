using System.Collections.Generic;

namespace Helteix.Tools.Phases
{
    internal sealed class DefaultPhaseListenerContainer<T> : IPhaseListenerContainer where T : IPhase
    {
        public int ExecutionOrder { get; }

        internal readonly IPhaseListener<T> listener;

        public DefaultPhaseListenerContainer(IPhaseListener<T> listener, int executionOrder)
        {
            ExecutionOrder = executionOrder;
            this.listener = listener;
        }

        public bool Accepts<TResult>(Phase<TResult> phase) => phase is T;

        public void OnPhaseBegin<TResult>(Phase<TResult> phase)
        {
            if(phase is not T t)
                return;

            listener.OnPhaseBegin(t);
        }

        public void OnPhaseEnd<TResult>(Phase<TResult> phase)
        {
            if(phase is not T t)
                return;

            listener.OnPhaseEnd(t);
        }

        public bool IsListener<TPhase>(IPhaseListener<TPhase> target) where TPhase : IPhase
        {
            return target == listener;
        }
    }
}