using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns.Arc
{
    public readonly partial struct AcrPatternWithData : IHexPattern<ArcPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll(ArcPatternData data, HexCoordinates from, IHexPatternController controller)
        {
            ArcPattern pattern = new ArcPattern(data.Center, data.To, data.Radius);
            return pattern.GetAll(from, controller);
        }
    }
}