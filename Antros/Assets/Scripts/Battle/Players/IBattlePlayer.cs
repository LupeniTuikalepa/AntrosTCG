using ATCG.Battle.Actions;
using UnityEngine;
using UnityEngine.InputSystem.Users;

namespace ATCG.Battle.Players
{
    public interface IBattlePlayer
    {
        public IBattlePlayerProfile Profile { get; }

        bool IsDefeated();

        Awaitable<BattleTurn> PlayTurn(int round, int turnNumber);
    }
}