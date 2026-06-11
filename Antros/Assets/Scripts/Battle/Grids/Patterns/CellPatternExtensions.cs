using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Patterns
{
    public static class CellPatternExtensions
    {
        public static void GetCellsFor<T>(this T pattern, BattleGrid battleGrid, List<HexCoordinates> output) where T : ICellPattern
            => GetCellsFor(pattern, battleGrid.grid, output);


        public static void GetCellsFor<T>(this T pattern, HexGrid grid, List<HexCoordinates> output)
            where T : ICellPattern
        {
            foreach (var t in pattern.GetAllCoordinates())
            {
                if (grid.Exists(t))
                    output.Add(t);
            }
        }

        public static void FillHashSet<T>(this T pattern, HashSet<HexCoordinates> output) where T : ICellPattern
        {
            foreach (var coord in pattern.GetAllCoordinates())
                output.Add(coord);
        }

        public static void Fill<T>(this T pattern, List<HexCoordinates> output) where T : ICellPattern
        {
            foreach (var coord in pattern.GetAllCoordinates())
                output.Add(coord);
        }
    }
}