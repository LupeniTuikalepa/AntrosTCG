using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct SpiralPatternWithData : IHexPattern<SpiralPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll(SpiralPatternData data, HexCoordinates from, IHexPatternController controller)
            => from.GetSpiral(data.Distance);
    }
}