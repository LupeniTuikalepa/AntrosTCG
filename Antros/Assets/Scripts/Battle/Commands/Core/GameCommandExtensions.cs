using ATCG.Battle.GameModes;
using UnityEngine;

namespace ATCG.Battle.Commands.Core
{
    public static class GameCommandExtensions
    {
        public static void RunAndForget<T>(this T command, BattlePhase battlePhase) where T : GameCommand =>
            Run(command, battlePhase);

        public static Awaitable Run<T>(this T command, BattlePhase battlePhase) where T : GameCommand
        {
            return GameCommandManager.Instance.ExecuteGameCommand(command, battlePhase);
        }
    }
}