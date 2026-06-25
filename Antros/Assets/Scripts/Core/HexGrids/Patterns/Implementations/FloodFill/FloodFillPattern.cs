using System.Collections.Generic;
using ATCG.HexGrids.Utility;
using UnityEngine.Pool;

namespace ATCG.HexGrids.Patterns
{
    public readonly partial struct FloodFillPattern : IHexPattern
    {
        public readonly int distance;

        public FloodFillPattern(int distance)
        {
            this.distance = distance;
        }

        public IEnumerable<HexCoordinates> GetAll(HexCoordinates from, IHexPatternController controller)
        {
            using (DictionaryPool<HexCoordinates, int>.Get(out Dictionary<HexCoordinates, int> dic))
            {
                FloodFill(from, distance, dic, controller);
                foreach ((HexCoordinates hexCoordinates, _) in dic)
                    yield return hexCoordinates;
            }
        }
        private void FloodFill<TController>(HexCoordinates coordinates, int budget, Dictionary<HexCoordinates, int> foundCells, TController controller)
            where TController : IHexPatternController
        {
            if (budget <= 0)
                return;

            foreach (HexCoordinates neighbor in coordinates.GetRing(1))
            {
                int newBudget = budget - 1;

                if (foundCells.TryGetValue(neighbor, out int lastBudget))
                    if (lastBudget >= newBudget)
                        continue;

                foundCells[neighbor] = newBudget;       // blocking cell recorded (included)

                if (controller.Blocks(neighbor))        // but don't propagate past it
                    continue;

                FloodFill(neighbor, newBudget, foundCells, controller);
            }
        }

    }
}