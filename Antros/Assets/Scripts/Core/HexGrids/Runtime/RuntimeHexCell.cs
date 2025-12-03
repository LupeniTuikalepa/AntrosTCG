using PrimeTween;
using UnityEngine;

namespace ATCG.HexGrids.Runtime
{
    public class RuntimeHexCell : MonoBehaviour
    {
        public RuntimeHexGrid RuntimeGrid { get; private set; }
        public HexCoordinates Coordinates { get; private set; }

        public HexCell Cell => RuntimeGrid.IsConnected && RuntimeGrid.Current.TryGetCell(Coordinates, out var cell) ? cell : null;

        private uint startLayerMask;

        private void Awake()
        {
        }

        public void Init(RuntimeHexGrid grid)
        {
            RuntimeGrid = grid;
        }

        public void Connect(HexCoordinates coordinates)
        {
            Coordinates = coordinates;
            transform.position = RuntimeGrid.GetPositionAt(coordinates);
        }

        public void Disconnect()
        {
            Destroy(gameObject);
        }

    }
}