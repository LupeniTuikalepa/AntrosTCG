using ATCG.Battle.Entities;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;

namespace ATCG.Battle.CapacitySystem.Core
{
    public readonly struct CapacityTarget
    {
        public readonly HexCoordinates coordinates;
        public readonly EntityAddress address;

        public CapacityTarget(HexCoordinates coordinates, EntityAddress address)
        {
            this.coordinates = coordinates;
            this.address = address;
        }
    }
}