

namespace Helteix.Tools.Phases
{
    public interface ISinglePhase : IPhase
    {
        string Channel { get; }

        bool AllowMultipleInstances => false;
        bool AllowQueuing => true;
    }
}