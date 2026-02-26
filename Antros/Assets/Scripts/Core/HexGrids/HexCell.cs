using System;
using System.Collections.Generic;
using ATCG.HexGrids.Grids;
using UnityEngine;

namespace ATCG.HexGrids
{
    public class HexCell
    {
        public IEnumerable<IHexMember> Members => members;

        public readonly HexGrid hexGrid;
        public readonly HexCoordinates coordinates;

        private HashSet<IHexMember> members = new();

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

        public void AddMember(IHexMember member)
        {
            if(member != null)
                members.Add(member);
        }

        public void RemoveMember(IHexMember member)
        {
            if(member != null)
                members.Remove(member);
        }

        public void Dispose()
        {

        }

    }
}