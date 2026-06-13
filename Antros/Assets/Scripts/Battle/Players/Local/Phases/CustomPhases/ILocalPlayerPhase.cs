using System.Threading;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface ILocalPlayerPhase : IPhase
    {
        LocalBattlePlayer LocalBattlePlayer { get; }
    }

    public abstract class LocalPlayerPhase<T> : Phase<T>, ILocalPlayerPhase
    {
        public LocalBattlePlayer LocalBattlePlayer { get; }

        public LocalPlayerPhase(LocalBattlePlayer localBattlePlayer)
        {
            LocalBattlePlayer = localBattlePlayer;
        }

    }
}