using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct FloodFillPatternWithData : IHexPattern<FloodFillPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll(FloodFillPatternData data, HexCoordinates from,
            IHexPatternController controller)
        {
            FloodFillPattern floodFillPattern = new FloodFillPattern(data.Distance);
            return floodFillPattern.GetAll(from, controller);
        }
    }
}