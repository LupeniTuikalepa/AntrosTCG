using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct SpreadPattern : IHexPattern
    {
        private readonly int distance;

        public SpreadPattern(int distance)
        {
            this.distance = distance;
        }

        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, TController controller) where TController : IHexPatternController
        {
            foreach (HexCoordinates coord in from.GetRing(distance))
            {
                foreach (HexCoordinates lineCoord in from.GetLine(coord))
                {
                    yield return lineCoord;
                    if (controller.Blocks(lineCoord))
                        break;
                }
            }
        }
    }
}