namespace Helteix.Tools.Phases
{
    /// <summary>
    /// Non-generic marker interface for phases.
    /// Do not implement this interface directly — extend <see cref="Phase{TResult}"/> or <see cref="Phase"/> instead.
    /// This interface exists solely to allow non-generic references to phases ('e.g.' collections, constraints).
    /// </summary>
    public interface IPhase { }
}