using ATCG.HexGrids.Grids;
using UnityEngine;

namespace ATCG.HexGrids.Shapes
{
    public readonly struct HexagonalShapeBuilder : IShapeBuilder
    {
        public readonly uint radius;

        public HexagonalShapeBuilder(uint radius)
        {
            this.radius = radius;
        }


        public void Build(HexGrid hexGrid)
        {
            int r = (int)radius;

            for (int q = -r; q <= r; q++) {
                int a = Mathf.Max(-r, -q - r);
                int b = Mathf.Min( r, -q + r);
                for (int i = a; i <= b; i++) {
                    hexGrid.AddCell(new HexCoordinates(q, i));
                }
            }
        }
    }
}