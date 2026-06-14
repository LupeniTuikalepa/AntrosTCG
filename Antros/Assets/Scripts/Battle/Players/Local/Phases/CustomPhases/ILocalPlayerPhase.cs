using System.Threading;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
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

        public BattlePhase BattlePhase => LocalBattlePlayer.BattlePhase;
        public BattleGrid BattleGrid => BattlePhase.BattleGrid;

        public LocalPlayerPhase(LocalBattlePlayer localBattlePlayer)
        {
            LocalBattlePlayer = localBattlePlayer;
        }

    }
}