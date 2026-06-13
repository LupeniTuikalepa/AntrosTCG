using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.Commands.GameCommands
{
    public readonly struct CapacitySetup
    {
        public readonly CapacityData data;
        public readonly HexCoordinates castPoint;

        public CapacitySetup(CapacityData data, HexCoordinates castPoint)
        {
            this.data = data;
            this.castPoint = castPoint;
        }
    }
}