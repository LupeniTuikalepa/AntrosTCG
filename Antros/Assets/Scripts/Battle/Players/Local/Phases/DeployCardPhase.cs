using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public class DeployCardPhase : IPhase<HexCoordinates>
    {
        private readonly BattleGrid battleGrid;
        public readonly IBattleCard card;
        private readonly LocalBattlePlayer player;

        public DeployCardPhase(LocalBattlePlayer player, BattleGrid battleGrid, IBattleCard card)
        {
            this.player = player;
            this.battleGrid = battleGrid;
            this.card = card;
        }

        async Awaitable<HexCoordinates> IPhase<HexCoordinates>.Execute(CancellationToken token)
        {
            using (ListPool<HexCoordinates>.Get(out List<HexCoordinates> list))
            {
                list.AddRange(battleGrid.AllCellsCoordinates);

                RemoveOccupiedCells(list);

                SelectCellPhase selectPhase = new(list, battleGrid, player);
                PhaseResult<HexCoordinates> result = await selectPhase.Run();

                if (result is { type: PhaseResultType.Success })
                    return result.value;

                return HexCoordinates.None;
            }
        }

        async Awaitable IPhase<HexCoordinates>.Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        async Awaitable IPhase<HexCoordinates>.Dispose(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private void RemoveOccupiedCells(List<HexCoordinates> list)
        {
            foreach (Entity entity in battleGrid.World.Query(Query.With<PhysicalCellMemberTag>()))
                if (entity.TryGetROComponent(battleGrid.World, out BattleGridElementComponent gridEntityComponent))
                    list.Remove(gridEntityComponent.coordinates);
        }
    }
}