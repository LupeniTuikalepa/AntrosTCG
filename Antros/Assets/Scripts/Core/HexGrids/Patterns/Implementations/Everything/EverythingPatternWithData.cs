using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct EverythingPatternWithData : IHexPattern<EverythingPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll(EverythingPatternData data, HexCoordinates from,
            IHexPatternController controller)
        {
            EverythingPattern everythingPattern = new EverythingPattern();
            return everythingPattern.GetAll(from, controller);
        }
    }
}