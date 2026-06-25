using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct PointsPatternWithData : IHexPattern<PointsPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll(PointsPatternData data, HexCoordinates from, IHexPatternController controller)
        {
            PointsPattern pointsPattern = new PointsPattern(data.Points);
            return pointsPattern.GetAll(from, controller);
        }
    }
}