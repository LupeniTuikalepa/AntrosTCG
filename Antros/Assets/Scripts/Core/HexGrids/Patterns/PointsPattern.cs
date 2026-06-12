using System.Collections.Generic;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids.Patterns
{
    public readonly struct PointsPattern : IHexPattern
    {
        private readonly HexCoordinates[] points;

        public PointsPattern(params HexCoordinates[] points)
        {
            this.points = points;
        }

        public IEnumerable<HexCoordinates> GetAll(HexCoordinates from)
        {
            for (int i = 0; i < points.Length; i++)
                yield return points[i];
        }
    }
}