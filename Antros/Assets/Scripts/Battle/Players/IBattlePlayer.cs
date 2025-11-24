using ATCG.Battle.Actions;
using UnityEngine;

namespace ATCG.Battle.Players
{
    public interface IBattlePlayer
    {
        public BattlePlayerProfile Profile { get; }

        bool IsDefeated();

        Awaitable<BattleTurn> PlayTurn(int round, int turnNumber);
    }
}