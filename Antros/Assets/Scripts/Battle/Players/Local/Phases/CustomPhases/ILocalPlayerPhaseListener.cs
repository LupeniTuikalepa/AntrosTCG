using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface ILocalPlayerPhaseListener<in T> : IPhaseListener<T> where T : IPhase, ILocalPlayerPhase
    {
        LocalBattlePlayer LocalBattlePlayer { get; }

        bool IPhaseListener<T>.Accepts(T phase)
        {
            return phase.LocalBattlePlayer == LocalBattlePlayer;
        }
    }
}