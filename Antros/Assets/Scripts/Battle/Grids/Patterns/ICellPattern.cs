using System;
using System.Collections.Generic;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids.Patterns
{
    public interface ICellPattern
    {
        public Func<HexCoordinates, bool> ValidateCell { set; }
        void Evaluate(List<HexCoordinates> coordinatesList);
    }
}