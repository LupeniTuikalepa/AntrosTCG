using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct PointsPattern : IHexPattern
    {
        private readonly HexCoordinates[] points;

        public PointsPattern(params HexCoordinates[] points)
        {
            this.points = points;
        }

        public IEnumerable<HexCoordinates> GetAll(HexCoordinates from, IHexPatternController controller)
        {
            for (int i = 0; i < points.Length; i++)
                yield return points[i];
        }
    }
}