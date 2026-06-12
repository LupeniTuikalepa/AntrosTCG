using System.Collections.Generic;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using Helteix.Tools.DataMapping;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Patterns
{
    public interface IHexPattern
    {
        IEnumerable<HexCoordinates> GetAll(HexCoordinates from);
    }
}