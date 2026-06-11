using System.Collections.Generic;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Patterns
{
    public interface ICellPattern
    {
        public sealed IEnumerable<HexCoordinates> GetAllCoordinates()
        {
            using (HashSetPool<HexCoordinates>.Get(out var output))
            {
                foreach (var coord in GetAll())
                {
                    //Ensures all coords are only sent once and never again
                    if(output.Add(coord))
                        yield return coord;
                }
            }
        }

        protected IEnumerable<HexCoordinates> GetAll();

    }
}