using ATCG.Battle.Cards;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.GameCommands
{
    public readonly struct Capacity
    {
        public readonly CapacityData data;
        public readonly HexCoordinates castPoint;
        public readonly IBattleCard card;
        public readonly BattleGrid grid;

        public Capacity(CapacityData data, HexCoordinates castPoint, IBattleCard card, BattleGrid grid)
        {
            this.data = data;
            this.castPoint = castPoint;
            this.card = card;
            this.grid = grid;
        }
    }
}