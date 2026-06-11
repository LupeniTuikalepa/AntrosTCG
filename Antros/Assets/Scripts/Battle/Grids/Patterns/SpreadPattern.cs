using System;
using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Patterns
{
    public struct SpreadPattern : ICellPattern
    {
        public readonly int distance;
        public readonly HexCoordinates center;

        public SpreadPattern(HexCoordinates center, int distance)
        {
            this.center = center;
            this.distance = distance;
        }


        IEnumerable<HexCoordinates> ICellPattern.GetAll()
        {
            //GetRef the max range ring
            foreach (HexCoordinates coord in center.GetRing(distance))
            {
                IEnumerable<HexCoordinates> line = center.GetLine(coord);
                //Adds every valid coord between the center and the extremity by drawing a line between.
                foreach (HexCoordinates lineCoord in line)
                    yield return lineCoord;
            }
        }
    }
}