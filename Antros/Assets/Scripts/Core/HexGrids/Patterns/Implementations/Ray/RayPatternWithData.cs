using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct RayPatternWithData : IHexPattern<RayPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, RayPatternData data,
            TController controller)
            where TController : IHexPatternController
        {
            RayPattern pattern = new RayPattern(data.Direction, data.Range);
            return pattern.GetAll(from, controller);
        }
    }
}