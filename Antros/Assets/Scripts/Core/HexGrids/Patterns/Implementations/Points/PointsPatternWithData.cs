using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct PointsPatternWithData : IHexPattern<PointsPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, PointsPatternData data, TController controller)
            where TController : IHexPatternController
        {
            PointsPattern pointsPattern = new PointsPattern(data.Points);
            return pointsPattern.GetAll(from, controller);
        }
    }
}