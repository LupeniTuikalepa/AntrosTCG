using ATCG.Battle.Entities.Components;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.HexGrids;

namespace ATCG.Battle.Entities.Aspects
{
    public readonly partial struct CapacityCasterAspect : IEntityAspect<GridMemberComponent, CapacityCasterComponent>
    {
        public CapacityData[] Capacities => CapacityCasterComponent.capacities;
        public HexCoordinates Coordinates => GridMemberComponent.coordinates;
        public BattleGrid BattleGrid => GridMemberComponent.grid;
    }
}