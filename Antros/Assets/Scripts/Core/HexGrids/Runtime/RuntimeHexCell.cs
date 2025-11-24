using PrimeTween;
using UnityEngine;

namespace ATCG.HexGrids.Runtime
{
    public class RuntimeHexCell : MonoBehaviour
    {
        public RuntimeHexGrid RuntimeGrid { get; private set; }
        public HexCoordinates Coordinates { get; private set; }
        public MeshRenderer MeshRenderer { get; private set; }


        public HexCell Cell => RuntimeGrid.IsConnected && RuntimeGrid.Current.TryGetCell(Coordinates, out var cell) ? cell : null;

        private uint startLayerMask;

        private void Awake()
        {
            MeshRenderer = GetComponent<MeshRenderer>();
            startLayerMask = MeshRenderer.renderingLayerMask;
        }

        public void Init(RuntimeHexGrid grid)
        {
            RuntimeGrid = grid;
        }

        public void Connect(HexCoordinates coordinates)
        {
            Coordinates = coordinates;
            transform.position = RuntimeGrid.GetPositionAt(coordinates);
            Vector3 targetScale = Vector3.one * RuntimeGrid.Current.OuterCellRadius * 1.8f;
            float delay = coordinates.Length() * .08f;
            Tween.Scale(transform, targetScale, new TweenSettings() { startDelay = delay, ease = Ease.OutElastic, duration = 1f});
            transform.localScale = Vector3.zero;
            transform.Rotate(Vector3.up, 30);
        }

        public void Disconnect()
        {
            Destroy(gameObject);
        }

    }
}