using System.Threading;
using System.Threading.Tasks;
using ATCG.Battle.Cards;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases.CardDeploy
{
    public class DeployCardPhase : IPhase<BattleCell>
    {
        private readonly LocalBattlePlayer player;
        private readonly BattleGrid battleGrid;
        public readonly IBattleCard card;

        public DeployCardPhase(LocalBattlePlayer player, BattleGrid battleGrid, IBattleCard card)
        {
            this.player = player;
            this.battleGrid = battleGrid;
            this.card = card;
        }

        async Awaitable<BattleCell> IPhase<BattleCell>.Execute(CancellationToken token)
        {
            using (ListPool<HexCoordinates>.Get(out var list))
            {
                foreach (var cell in battleGrid.GetCells())
                {
                    if (cell.CanBeDeployedOn(player))
                        list.Add(cell.cell.coordinates);
                }

                var selectPhase = new SelectCellPhase(list, battleGrid, player);
                PhaseResult<BattleCell> result = await selectPhase.Run();

                if (result is { type: PhaseResultType.Success })
                    return result.value;

                return null;
            }
        }

        async Awaitable IPhase<BattleCell>.Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        async Awaitable IPhase<BattleCell>.Dispose(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }
}