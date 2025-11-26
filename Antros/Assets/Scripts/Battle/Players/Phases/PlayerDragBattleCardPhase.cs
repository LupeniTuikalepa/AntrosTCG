using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ATCG.Battle.Cards;
using ATCG.Battle.HexGrids;
using ATCG.HexGrids;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players
{
    public sealed class PlayerDragBattleCardPhase : PhaseCompletionSource<BattleCell>
    {
        public interface ICellFilter
        {
            bool Accepts(BattleGrid grid, HexCoordinates coordinates, IBattleCard battleCard);
        }

        private readonly ICellFilter filter;
        public readonly BattleGrid battleGrid;
        public readonly IBattleCard card;

        private List<HexCoordinates> validCells;

        public IReadOnlyList<HexCoordinates> ValidCells => validCells;

        public PlayerDragBattleCardPhase(ICellFilter filter, BattleGrid battleGrid, IBattleCard card)
        {
            this.filter = filter;
            this.battleGrid = battleGrid;
            this.card = card;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            validCells = ListPool<HexCoordinates>.Get();
            foreach (BattleCell battleCell in  battleGrid.GetCells())
            {
                if (filter.Accepts(battleGrid, battleCell.cell.coordinates, card))
                    validCells.Add(battleCell.cell.coordinates);
            }

            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            ListPool<HexCoordinates>.Release(validCells);
            return base.Dispose(token);
        }

        public bool IsCoordValid(HexCoordinates coord) => validCells.Contains(coord);

    }
}