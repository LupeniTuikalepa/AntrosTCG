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

        public int Size => Cells.Count;

        private Dictionary<HexCoordinates, HexCell> cells;
        private Dictionary<HexCoordinates, HexCell> Cells => cells??=new();

        public IEnumerable<HexCoordinates> CellsCoordinates => cells.Keys;
        public IEnumerable<HexCell> CellsValues => cells.Values;

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

        public bool AddCell(HexCoordinates coordinates)
        {
            if (!coordinates.IsValid)
                return false;

            //Debug.Log("Adding new cell");
            if (Cells.ContainsKey(coordinates))
                RemoveCell(coordinates);

            HexCell cell = new HexCell(this, coordinates);
            Cells.Add(coordinates, cell);
            OnCellAdded?.Invoke(cell);
            return true;
        }

        private bool RemoveCell(HexCoordinates coordinates)
        {
            if (!coordinates.IsValid)
                return false;

            if (Cells.Remove(coordinates, out HexCell cell))
            {
                OnCellRemoved?.Invoke(cell);
                cell.Dispose();
                return true;
            }

            return false;
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

        public HexCell GetCell(HexCoordinates coordinates) => TryGetCell(coordinates, out var cell) ? cell : null;
        public bool TryGetCell(HexCoordinates coordinates, out HexCell cell)
        {
            if (!coordinates.IsValid)
            {
                cell = null;
                return false;
            }

            return Cells.TryGetValue(coordinates, out cell);
        }

        public IEnumerable<HexCell> GetCells() => Cells.Values;

        public IEnumerable<HexCoordinates> GetCoords() => Cells.Keys;

        IEnumerator<HexCell> IEnumerable<HexCell>.GetEnumerator() => Cells.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Cells.Values.GetEnumerator();
    }
}