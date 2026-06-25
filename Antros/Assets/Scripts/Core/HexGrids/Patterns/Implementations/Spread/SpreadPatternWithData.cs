using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct SpreadPatternWithData : IHexPattern<SpreadPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll(SpreadPatternData data, HexCoordinates from, IHexPatternController controller)
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