using System.Collections.Generic;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Grids;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public struct WalkingPathGenerator : IPathGenerator
    {
        private const int MAX_STEPS = 10000;

        public IEnumerable<HexCoordinates> GetPathBetween(HexCoordinates a, HexCoordinates b, PathGenerationContext context)
        {
            using var hexPathfinder = new HexPathfinder(MAX_STEPS);
            using (ListPool<HexCoordinates>.Get(out var path))
            {
                hexPathfinder.FindPath(a, b, path, context.battleGrid);

                foreach (var coord in path)
                {
                    yield return coord;
                }
            }
        }
    }
}