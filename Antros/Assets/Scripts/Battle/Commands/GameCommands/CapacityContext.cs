using ATCG.Battle.Cards;
using ATCG.Battle.Entities;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.GameCommands
{
    public readonly struct CapacityContext
    {
        public readonly CapacityData data;
        public readonly HexCoordinates castPoint;

        public CapacityContext(CapacityData data, HexCoordinates castPoint)
        {
            this.data = data;
            this.castPoint = castPoint;
        }
    }
}