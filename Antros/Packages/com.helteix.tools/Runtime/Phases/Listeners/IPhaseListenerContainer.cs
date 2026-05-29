namespace Helteix.Tools.Phases
{
    internal interface IPhaseListenerContainer
    {
        int ExecutionOrder { get; }
        bool Accepts<TResult>(Phase<TResult> phase);


        void OnPhaseBegin<TResult>(Phase<TResult> phase);

        void OnPhaseEnd<TResult>(Phase<TResult> phase);
    }
}