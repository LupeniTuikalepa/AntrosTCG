using Helteix.Tools.Phases;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface ILocalPlayerPhaseListener<in T> : IPhaseListener<T> where T : IPhase, ILocalPlayerPhase
    {
        LocalBattlePlayer LocalBattlePlayer { get; }

        bool IPhaseListener<T>.Accepts(T phase) => phase.LocalBattlePlayer == LocalBattlePlayer;
    }
}