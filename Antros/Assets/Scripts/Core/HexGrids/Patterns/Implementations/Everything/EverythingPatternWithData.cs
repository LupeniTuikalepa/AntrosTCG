using System.Collections.Generic;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct EverythingPatternWithData : IHexPattern<EverythingPatternData>
    {
        public IEnumerable<HexCoordinates> GetAll<TController>(HexCoordinates from, EverythingPatternData data, TController controller)
            where TController : IHexPatternController
        {
            EverythingPattern everythingPattern = new EverythingPattern();
            return everythingPattern.GetAll(from, controller);
        }
    }
}