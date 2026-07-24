using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids
{
    public struct PathSegment
    {
        public HexCoordinates from;
        public HexCoordinates to;
        public AgentMovementType movementType;
    }
}