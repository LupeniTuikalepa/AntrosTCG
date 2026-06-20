using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct RayPattern : IHexPattern
    {
        public readonly HexDirection direction;
        public readonly int distance;

        public RayPattern(HexDirection direction, int distance)
        {
            this.direction = direction;
            this.distance = distance;
        }

        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, TController controller) where TController : IHexPatternController
        {
            HexCoordinates dir = HexOperations.GetDirection(direction);
            HexCoordinates destination = dir * distance;

            return from.GetLine(from + destination);
        }
    }
}