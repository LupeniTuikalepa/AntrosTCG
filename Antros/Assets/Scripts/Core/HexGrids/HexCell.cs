using System;
using System.Collections.Generic;
using ATCG.HexGrids.Grids;
using UnityEngine;

namespace ATCG.HexGrids
{
    public class HexCell
    {

        public readonly HexGrid hexGrid;
        public readonly HexCoordinates coordinates;


        public HexCell(HexGrid hexGrid, HexCoordinates coordinates)
        {
            this.hexGrid = hexGrid;
            this.coordinates = coordinates;
        }


        public IEnumerable<Vector2> GetWorldCorners()
        {
            Vector2 center = hexGrid.GetPositionAt(coordinates);
            foreach (Vector2 corner in GetLocalCorners())
                yield return corner + center;
        }

        public IEnumerable<Vector2> GetLocalCorners()
        {
            yield return new Vector2(0f, hexGrid.OuterCellRadius);
            yield return new Vector2(hexGrid.InnerRadius, 0.5f * hexGrid.OuterCellRadius);
            yield return new Vector2(hexGrid.InnerRadius, -0.5f * hexGrid.OuterCellRadius);
            yield return new Vector2(0f, -hexGrid.OuterCellRadius);
            yield return new Vector2(-hexGrid.InnerRadius, -0.5f * hexGrid.OuterCellRadius);
            yield return new Vector2(-hexGrid.InnerRadius, 0.5f * hexGrid.OuterCellRadius);
        }


        public void Dispose()
        {

        }

    }
}