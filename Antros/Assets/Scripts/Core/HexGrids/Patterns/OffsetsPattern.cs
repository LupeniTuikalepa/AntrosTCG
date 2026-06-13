using System.Collections.Generic;
using ATCG.HexGrids;

namespace ATCG.Battle.Grids.Patterns
{
    public readonly struct OffsetsPattern : IHexPattern
    {
        private readonly HexCoordinates[] offsets;

        public OffsetsPattern(params HexCoordinates[] offsets)
        {
            this.offsets = offsets;
        }

        public IEnumerable<HexCoordinates> GetAll(HexCoordinates from)
        {
            for (int i = 0; i < offsets.Length; i++)
                yield return from + offsets[i];
        }
    }
}