namespace Helteix.Tools.Phases
{
    public interface IPhaseListener<in T>
        where T : IPhase
    {
        void OnPhaseBegin(T phase);

        void OnPhaseEnd(T phase);

    }
}