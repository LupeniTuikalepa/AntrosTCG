using System;
using System.Collections;
using System.Collections.Generic;
using ATCG.HexGrids.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.HexGrids.Grids
{
    [System.Serializable]
    public class HexGrid : IEnumerable<HexCell>
    {
        public event Action<HexCell> OnCellAdded;
        public event Action<HexCell> OnCellRemoved;
        public float InnerRadius => OuterCellRadius * 0.866025404f;
        public float OuterCellRadius { get; private set; }
        public Vector2 Center { get; private set; }

        private Dictionary<HexCoordinates, HexCell> cells;
        private Dictionary<HexCoordinates, HexCell> Cells => cells??=new();


        public HexGrid(float outerCellRadius = 10f, Vector2 center = default)
        {
            OuterCellRadius = outerCellRadius;
            Center = center;
        }

        public Vector2 GetPositionAt(HexCoordinates coordinates)
        {
            int x = coordinates.X;
            int y = coordinates.Y;

            Vector2 position = new()
            {
                x =  (x + y * 0.5f) * (InnerRadius * 2f),
                //y = (x + y * 0.5f) * (InnerRadius * 2f)
                y = y * (OuterCellRadius * 1.5f)
            };

            return position + Center;
        }

        public void AddCell(HexCoordinates coordinates)
        {
            //Debug.Log("Adding new cell");
            if (Cells.ContainsKey(coordinates))
                RemoveCell(coordinates);

            HexCell cell = new HexCell(this, coordinates);
            Cells.Add(coordinates, cell);
            OnCellAdded?.Invoke(cell);
        }

        private void RemoveCell(HexCoordinates coordinates)
        {
            if (Cells.Remove(coordinates, out HexCell cell))
            {
                OnCellRemoved?.Invoke(cell);
                cell.Dispose();
            }
        }


        public void Clear()
        {
            using (ListPool<HexCoordinates>.Get(out var list))
            {
                list.AddRange(Cells.Keys);
                foreach (HexCoordinates coord in list)
                    RemoveCell(coord);
            }
        }


        #region Utility


        public IEnumerable<HexCoordinates> GetSpiral(HexCoordinates center, int radius)
        {
            for (int k = 1; k <= radius; k++)
            {
                foreach (var coord in GetRing(center, k))
                    yield return coord;
            }
        }

        public IEnumerable<HexCoordinates> GetRing(HexCoordinates center, int radius)
        {
            if (radius <= 0)
                yield return center;

            HexCoordinates hex =  HexOperations.Offset(center, HexDirection.BottomLeft, radius);

            for (int direction = 0; direction < 6; direction++)
            {
                for (int j = 0; j < radius; j++)
                {
                    yield return hex;
                    hex = HexOperations.Offset(hex, (HexDirection)direction);
                }
            }
        }

        #endregion
        #region Members

        public bool HasMember(HexCoordinates coordinates)
            => HasMember<IHexMember>(coordinates);
        public bool HasMember<T>(HexCoordinates coordinates) where T : IHexMember
            => TryGetHexMember<T>(coordinates, out _);
        public bool TryGetHexMember(HexCoordinates coordinates, out IHexMember member)
            => TryGetHexMember<IHexMember>(coordinates, out member);
        public bool TryGetHexMember<T>(HexCoordinates coordinates, out T member) where T : IHexMember
        {
            if (TryGetCell(coordinates, out HexCell cell) && cell.Members is T m)
            {
                member = m;
                return true;
            }

            member = default;
            return false;
        }

        public IEnumerable<T> GetMembers<T>() where T : IHexMember
        {
            foreach (var member in GetMembers())
            {
                if (member is T hexMember)
                    yield return hexMember;
            }
        }

        public IEnumerable<IHexMember> GetMembers()
        {
            foreach (HexCell hexCell in cells.Values)
            {
                if (hexCell.Members == null)
                    continue;

                foreach (var member in hexCell.Members)
                    yield return member;
            }

        }

        #endregion

        public HexCell GetCell(HexCoordinates coordinates) => TryGetCell(coordinates, out var cell) ? cell : null;
        public bool TryGetCell(HexCoordinates coordinates, out HexCell cell) => Cells.TryGetValue(coordinates, out cell);

        public IEnumerable<HexCell> GetCells() => Cells.Values;

        public IEnumerable<HexCoordinates> GetCoords() => Cells.Keys;

        IEnumerator<HexCell> IEnumerable<HexCell>.GetEnumerator() => Cells.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Cells.Values.GetEnumerator();
    }
}