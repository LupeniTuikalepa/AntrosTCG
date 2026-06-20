using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct FloodFillPatternWithData : IHexPattern<FloodFillPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, FloodFillPatternData data, TController controller)
            where TController : IHexPatternController
        {
            FloodFillPattern floodFillPattern = new FloodFillPattern(data.Distance);
            return floodFillPattern.GetAll(from, controller);
        }

    }
}