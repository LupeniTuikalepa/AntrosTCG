using System;

namespace Helteix.Tools.Phases.Listeners
{
    internal class CallbackPhaseListener<T>: IPhaseListenerContainer where T : IPhase
    {
        internal readonly Action<T> beginCallback;
        internal readonly Action<T> endCallback;
        public int ExecutionOrder { get; }
        public CallbackPhaseListener(Action<T> beginCallback, Action<T> endCallback, int executionOrder = 0)
        {
            ExecutionOrder = executionOrder;
            this.beginCallback = beginCallback;
            this.endCallback = endCallback;
        }

        public bool Accepts<TResult>(Phase<TResult> phase) => phase is T;

        public void OnPhaseBegin<TResult>(Phase<TResult> phase)
        {
            if(phase is not T t)
                return;

            beginCallback(t);
        }

        public void OnPhaseEnd<TResult>(Phase<TResult> phase)
        {
            if(phase is not T t)
                return;

            endCallback(t);
        }

    }
}