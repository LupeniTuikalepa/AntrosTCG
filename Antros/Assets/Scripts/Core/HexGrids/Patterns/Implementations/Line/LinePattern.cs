using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct LinePattern: IHexPattern
    {
        private readonly HexCoordinates start;
        private readonly HexCoordinates end;
        private readonly bool hasExtremity;

        public LinePattern(HexCoordinates start, HexCoordinates end)
        {
            this.start = start;
            this.end = end;
            hasExtremity = true;
        }

        public LinePattern(HexCoordinates end)
        {
            this.start = end;
            this.end = end;

            hasExtremity = false;
        }

        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, TController controller)
            where TController : IHexPatternController
        {
            HexCoordinates a = hasExtremity ? start : from;
            HexCoordinates b = end;

            foreach (var point in a.GetLine(b))
            {
                yield return point;
                if(controller.Blocks(point))
                    break;
            }
        }
    }
}