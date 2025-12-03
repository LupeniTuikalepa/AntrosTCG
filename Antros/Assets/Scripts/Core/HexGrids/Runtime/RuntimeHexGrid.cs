using System;
using System.Collections.Generic;
using ATCG.HexGrids.Grids;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.HexGrids.Runtime
{
    public class RuntimeHexGrid : MonoBehaviour
    {
        public event Action<RuntimeHexCell> OnCellAdded;
        public event Action<RuntimeHexCell> OnCellRemoved;
        public bool IsConnected => Current != null;
        public HexGrid Current { get; private set; }

        private Dictionary<HexCoordinates, RuntimeHexCell> cells = new();

        [SerializeField]
        private RuntimeHexCell prefab;

        public void Connect(HexGrid grid)
        {
            Disconnect();
            Current = grid;
            Current.OnCellAdded += AddCell;
            Current.OnCellRemoved += RemoveCell;

            foreach (var cell in grid.GetCells())
                AddCell(cell);
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;

            Current.OnCellAdded -= AddCell;
            Current.OnCellRemoved -= RemoveCell;
            using (ListPool<RuntimeHexCell>.Get(out var list))
            {
                list.AddRange(cells.Values);
                foreach (var runtimeHexCell in list)
                {
                    runtimeHexCell.Disconnect();
                }
            }

            Current = null;
        }

        private void AddCell(HexCell cell)
        {
            if (!cells.TryGetValue(cell.coordinates, out RuntimeHexCell view))
            {
                view = Instantiate(prefab, transform);
                view.Init(this);
            }
            else
                view.Disconnect();

            //Debug.Log("Adding cell " + cell.coordinates);
            view.Connect(cell.coordinates);
            OnCellAdded?.Invoke(view);
        }

        private void RemoveCell(HexCell cell)
        {
            if (cells.Remove(cell.coordinates, out RuntimeHexCell view))
            {
                OnCellRemoved?.Invoke(view);
                view.Disconnect();
            }
        }

        public Vector2 GetPositionAt(HexCoordinates coordinates) => Current.GetPositionAt(coordinates);
    }
}