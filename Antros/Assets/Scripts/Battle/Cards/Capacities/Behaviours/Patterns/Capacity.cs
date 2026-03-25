using System;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.Cards.Capacities.Patterns
{
    //TODO
    public readonly struct Capacity : IDisposable
    {
        public readonly HexCoordinates origin;
        public readonly BattleGrid battleGrid;
        public readonly CapacityData capacityData;
        public readonly IBattleCard card;

        /*
        public Capacity(BattleGrid battleGrid, IBattleCard card, CapacityData capacityData) : this(battleGrid, card, card.Coordinates, capacityData)
        {

        }
        */

        public Capacity(BattleGrid battleGrid, IBattleCard card, HexCoordinates origin, CapacityData capacityData)
        {
            this.battleGrid = battleGrid;
            this.card = card;
            this.origin = origin;
            this.capacityData = capacityData;
        }

        public void Dispose()
        {
        }
    }
}