using System.Collections.Generic;
using ATCG.Battle.Grids;
using ATCG.Battle.Metrics;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local.Phases.Filters;
using ATCG.HexGrids;
using Helteix.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Cards.Capacities
{
    public readonly struct BasicAttackEvent : ICardEvent<IBasicAttackEventRunner>
    {
        public readonly HeroBattleCard heroBattleCard;
        public readonly BattleGrid battleGrid;
        public readonly IBattlePlayer battlePlayer;

        public BasicAttackEvent(HeroBattleCard heroBattleCard, IBattlePlayer battlePlayer, BattleGrid battleGrid)
        {
            this.heroBattleCard = heroBattleCard;
            this.battlePlayer = battlePlayer;
            this.battleGrid = battleGrid;
        }

        public async Awaitable Run(List<IBasicAttackEventRunner> runners)
        {
            SpreadCellFilter spreadCellFilter = new(GameplayMetrics.Current.BasicAttackRange, heroBattleCard.Coordinates);
            battlePlayer.AddOrRemoveMana(-GameplayMetrics.Current.BasicAttackCost);

            using (ListPool<Awaitable>.Get(out List<Awaitable> awaitables))
            {
                foreach (IBasicAttackEventRunner runner in runners)
                    awaitables.Add(runner.BeginBasicAttack(this));

                await awaitables.WhenAll();
            }

            using (ListPool<HexCoordinates>.Get(out List<HexCoordinates> results))
            {
                battleGrid.SearchThroughGrid(spreadCellFilter, results);
                foreach (HexCoordinates coordinates in results)
                {
                    if (!battleGrid.TryGetBattleCell(coordinates, out BattleCell battleCell))
                        continue;
                    battleCell.OnBasicAttackPerformed(heroBattleCard);

                    foreach (IHexMember member in battleCell.cell.Members)
                    {
                        if (member is not HeroBattleCard card)
                            continue;
                        if (card == heroBattleCard)
                            continue;

                        card.AddOrRemoveHealth(-heroBattleCard.Strength);
                    }
                }
            }
        }
    }
}