using ATCG.Battle.Entities;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local;
using UnityEngine;

namespace ATCG.Battle
{
    public abstract class EntityAction
    {
        protected readonly LocalBattlePlayer fromPlayer;

        public abstract int ManaCost { get; }

        protected EntityAction(LocalBattlePlayer fromPlayer)
        {
            this.fromPlayer = fromPlayer;
        }

        public abstract Awaitable Execute(EntityAddress address, BattlePhase battlePhase);
    }
}