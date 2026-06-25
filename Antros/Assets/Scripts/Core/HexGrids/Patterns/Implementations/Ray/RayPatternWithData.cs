using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct RayPatternWithData : IHexPattern<RayPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll(RayPatternData data, HexCoordinates from, IHexPatternController controller)
        {
            RayPattern pattern = new RayPattern(data.Direction, data.Range);
            return pattern.GetAll(from, controller);
        }
    }
}