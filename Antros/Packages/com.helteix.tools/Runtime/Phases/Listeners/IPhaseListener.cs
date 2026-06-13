namespace Helteix.Tools.Phases
{
    public interface IPhaseListener<in T>
        where T : IPhase
    {
        bool Accepts(T phase) => true;

        void OnPhaseBegin(T phase);

        void OnPhaseEnd(T phase);

    }
}