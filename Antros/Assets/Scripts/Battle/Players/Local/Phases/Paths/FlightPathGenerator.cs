using System.Collections.Generic;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public struct FlightPathGenerator : IPathGenerator
    {
        private const int MAX_STEPS = 10000;

        public IEnumerable<HexCoordinates> GetPathBetween(HexCoordinates a, HexCoordinates b, HexCoordinates heroCoordinates, PathGenerationContext context)
        {
            return a.GetLine(b);
        }
    }
}