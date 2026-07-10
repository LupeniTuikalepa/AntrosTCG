using System.Collections.Generic;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public struct TeleportationPathGenerator : IPathGenerator
    {
        private const int MAX_STEPS = 10000;

        public IEnumerable<HexCoordinates> GetPathBetween(HexCoordinates a, HexCoordinates b, HexCoordinates heroCoordinates, PathGenerationContext context)
        {
            yield return a;
            yield return b;
        }
    }
}