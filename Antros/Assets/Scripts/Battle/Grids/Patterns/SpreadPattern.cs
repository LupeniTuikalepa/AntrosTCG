using System;
using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Utility;

namespace ATCG.Battle.Grids.Patterns
{
    public class SpreadPattern : ICellPattern
    {
        public Func<HexCoordinates, bool> ValidateCell { get; set; }

        public readonly int distance;
        public readonly HexCoordinates center;

        public SpreadPattern(HexCoordinates center, int distance)
        {
            this.center = center;
            this.distance = distance;
        }

        void ICellPattern.Evaluate(List<HexCoordinates> coordinatesList)
        {
            //GetRef the max range ring
            foreach (HexCoordinates coord in center.GetRing(distance))
            {
                IEnumerable<HexCoordinates> line = center.GetLine(coord);
                //Adds every valid coord between the center and the extremity by drawing a line between.
                foreach (HexCoordinates lineCoord in line)
                {
                    if(coordinatesList.Contains(lineCoord))
                        continue;

                    //Once an invalid cell is encountered, the line evaluation stops
                    if (ValidateCell != null && !ValidateCell(coord))
                        break;

                    coordinatesList.Add(lineCoord);
                }
            }
        }
    }
}