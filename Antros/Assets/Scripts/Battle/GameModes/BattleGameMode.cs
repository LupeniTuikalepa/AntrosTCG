using System.Threading;
using System.Threading.Tasks;
using ATCG.GameModes;
using UnityEngine;

namespace ATCG.Battle
{
    public class BattleGameMode : GameMode<BattleGameModeResults>
    {


        protected override async Awaitable Initialize()
        {
            await Task.CompletedTask;
        }


        protected override async Awaitable<BattleGameModeResults> Execute(CancellationToken token)
        {
            await Task.CompletedTask;
            return default;
        }

        protected override async Awaitable Dispose()
        {
            await Task.CompletedTask;
        }
    }
}