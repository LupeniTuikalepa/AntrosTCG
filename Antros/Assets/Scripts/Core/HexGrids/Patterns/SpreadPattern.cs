using System;
using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Patterns
{
    public readonly struct SpreadPattern : IHexPattern
    {
        public readonly int distance;

        public SpreadPattern(int distance)
        {
            this.distance = distance;
        }


        IEnumerable<HexCoordinates> IHexPattern.GetAll(HexCoordinates from)
        {
            //GetRef the max range ring
            foreach (HexCoordinates coord in from.GetRing(distance))
            {
                IEnumerable<HexCoordinates> line = from.GetLine(coord);
                //Adds every valid coord between the center and the extremity by drawing a line between.
                foreach (HexCoordinates lineCoord in line)
                    yield return lineCoord;
            }
        }
    }
}