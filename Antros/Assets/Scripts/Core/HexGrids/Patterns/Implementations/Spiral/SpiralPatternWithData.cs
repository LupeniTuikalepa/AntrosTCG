using System.Collections.Generic;
using ATCG.HexGrids.Utility;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct SpiralPatternWithData : IHexPattern<SpiralPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, SpiralPatternData data, TController controller)
            where TController : IHexPatternController
            => from.GetSpiral(data.Distance);
    }
}