using ATCG.Battle.GameModes;
using UnityEngine;

namespace ATCG.Battle.Commands.Core
{
    public static class GameCommandExtensions
    {
        public static Awaitable Execute<T>(this T command, BattlePhase battlePhase) where T : GameCommand
        {
            return GameCommandManager.Instance.ExecuteGameCommand(command, battlePhase);
        }
    }
}