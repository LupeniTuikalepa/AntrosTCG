using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct SpreadPatternWithData : IHexPattern<SpreadPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, SpreadPatternData data, TController controller)
            where TController : IHexPatternController
        {
            foreach (HexCoordinates coord in from.GetRing(data.Distance))
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