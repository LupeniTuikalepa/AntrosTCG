using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct LinePatternWithData : IHexPattern<LinePatternData>
    {
        public IEnumerable<HexCoordinates> GetAll(LinePatternData data, HexCoordinates from, IHexPatternController controller)
        {
            HexCoordinates a = data.IsAbsolute ? data.A : from + data.A;
            HexCoordinates b = data.IsAbsolute ? data.B : from + data.B;
            LinePattern linePattern = data.UseA ? new LinePattern(a, b) : new LinePattern(b);

            return linePattern.GetAll(from, controller);
        }
    }
}