using ATCG.HexGrids;

namespace ATCG.Battle.Grids.Controllers
{
    public class BattleIgnoreOriginPatternController : BattlePatternController
    {
        private readonly HexCoordinates origin;

        public BattleIgnoreOriginPatternController(BattleGrid battleGrid, HexCoordinates origin) : base(battleGrid)
        {
            this.origin = origin;
        }


        /// <summary>
        /// True if propagation stops at this coordinate. Branch onto your real
        /// </summary>
        public override bool Blocks(HexCoordinates c)
        {
            if (!battleGrid.TryGetBattleCell(c, out var cell))
                return true;

            if (origin == c)
                return false;

            return !cell.CanBeMovedOn();
        }
    }
}