using System;
using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids
{
    /// <summary>
    /// The single source of truth for "where can this agent go this turn, and by which path" —
    /// redirect slides already folded in. Build one with <see cref="HexPathfinder.ComputeReachable"/>
    /// at the start of a movement step (or an AI query); every consumer — highlight rings, hover
    /// preview, path commit, AI targeting — then reads THIS map, so redirect handling can never
    /// diverge between "reachable" and "the path shown/taken".
    ///
    /// A tile is a KEY (a reachable target) iff it is a real landing tile within budget. Tiles that
    /// a redirect merely slid the unit across appear inside some path but are NOT themselves keys:
    /// they are not standalone destinations. The full origin→tile path of every reachable tile is
    /// baked once at build time, so <see cref="TryGetPathFor"/> is an O(1) lookup — hovering never
    /// recomputes anything.
    /// </summary>
    public sealed class ReachableMap : IDisposable
    {
        // landing tile -> step cost from origin (origin = 0).
        private readonly Dictionary<HexCoordinates, int> costSoFar;
        // landing tile -> the accepted move that reached it (parent + traversed slides).
        private readonly Dictionary<HexCoordinates, MovementStep> cameFrom;
        // landing tile -> full path, origin inclusive .. tile inclusive, redirect slides included.
        private readonly Dictionary<HexCoordinates, HexCoordinates[]> fullPaths;

        public HexCoordinates Origin { get; }

        internal ReachableMap(HexCoordinates origin)
        {
            Origin = origin;
            costSoFar = DictionaryPool<HexCoordinates, int>.Get();
            cameFrom = DictionaryPool<HexCoordinates, MovementStep>.Get();
            fullPaths = DictionaryPool<HexCoordinates, HexCoordinates[]>.Get();
            costSoFar[origin] = 0;
        }

        // --- Build-time surface (used only by HexPathfinder while flooding) --------------------

        internal bool TryGetRawCost(HexCoordinates tile, out int cost) => costSoFar.TryGetValue(tile, out cost);

        internal void Record(HexCoordinates landing, int cost, MovementStep step)
        {
            costSoFar[landing] = cost;
            cameFrom[landing] = step;
        }

        /// <summary>
        /// Reconstructs and caches the full origin→tile path for every reachable landing tile.
        /// Called once by <see cref="HexPathfinder.ComputeReachable"/> after the flood completes.
        /// </summary>
        internal void BakePaths()
        {
            foreach (KeyValuePair<HexCoordinates, int> kv in costSoFar)
            {
                HexCoordinates tile = kv.Key;
                if (tile == Origin)
                    continue;

                using (ListPool<HexCoordinates>.Get(out var scratch))
                {
                    scratch.Add(Origin);
                    if (HexPathfinder.AppendReconstruct(Origin, tile, cameFrom, scratch))
                        fullPaths[tile] = scratch.ToArray();
                }
            }
        }

        // --- Read surface (every consumer) ------------------------------------------------------

        /// <summary>True when at least one tile other than the origin can be reached.</summary>
        public bool HasReachableTiles => costSoFar.Count > 1;

        /// <summary>The step cost of every reachable tile (origin included, cost 0). Read-only.</summary>
        public IReadOnlyDictionary<HexCoordinates, int> Costs => costSoFar;

        public bool IsReachable(HexCoordinates tile) => tile != Origin && costSoFar.ContainsKey(tile);

        public bool TryGetCost(HexCoordinates tile, out int cost) => costSoFar.TryGetValue(tile, out cost);

        /// <summary>
        /// Writes the precomputed shortest path to <paramref name="tile"/> — origin inclusive,
        /// tile inclusive, redirect slides included — into <paramref name="path"/> (cleared first).
        /// Returns false and leaves <paramref name="path"/> empty when the tile is not a reachable
        /// landing tile (unreachable, out of budget, or only slid across by a redirect).
        /// </summary>
        public bool TryGetPathFor(HexCoordinates tile, List<HexCoordinates> path)
        {
            path.Clear();
            if (!fullPaths.TryGetValue(tile, out HexCoordinates[] full))
                return false;

            path.AddRange(full);
            return true;
        }

        /// <summary>
        /// Like <see cref="TryGetPathFor"/>, but if <paramref name="goal"/> is not itself reachable
        /// (e.g. it is occupied by the very enemy an AI is chasing) falls back to the path to the
        /// reachable landing tile with the smallest hex distance to the goal. Returns false only
        /// when nothing at all is reachable.
        /// </summary>
        public bool TryGetPathToward(HexCoordinates goal, List<HexCoordinates> path)
        {
            if (TryGetPathFor(goal, path))
                return true;

            HexCoordinates best = HexCoordinates.None;
            int bestDistance = int.MaxValue;

            foreach (KeyValuePair<HexCoordinates, int> kv in costSoFar)
            {
                if (kv.Key == Origin)
                    continue;

                int distance = kv.Key.Distance(goal);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = kv.Key;
                }
            }

            if (!best.IsValid)
            {
                path.Clear();
                return false;
            }

            return TryGetPathFor(best, path);
        }

        public void Dispose()
        {
            DictionaryPool<HexCoordinates, int>.Release(costSoFar);
            DictionaryPool<HexCoordinates, MovementStep>.Release(cameFrom);
            DictionaryPool<HexCoordinates, HexCoordinates[]>.Release(fullPaths);
        }
    }
}
