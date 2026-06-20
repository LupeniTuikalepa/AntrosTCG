using System.Collections.Generic;
using ATCG.HexGrids;

namespace ATCG.Battle.Players.Local.Phases
{
    public interface IPathGenerator
    {
        IEnumerable<HexCoordinates> GetPathBetween(HexCoordinates a, HexCoordinates b, PathGenerationContext context);
    }
}