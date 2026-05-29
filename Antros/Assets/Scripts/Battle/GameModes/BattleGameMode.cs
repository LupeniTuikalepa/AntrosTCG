using System.Threading;
using System.Threading.Tasks;
using ATCG.GameModes;
using UnityEngine;

namespace ATCG.Battle.GameModes
{
    public class BattleGameMode : GameMode<BattleGameModeResults>
    {

        protected override async Awaitable<BattleGameModeResults> Execute(CancellationToken token)
        {
            await Task.CompletedTask;
            return default;
        }

    }
}