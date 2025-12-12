using ATCG.Battle.Metrics;
using ATCG.Battle.Players.Local.Phases;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using Helteix.Tools.Phases;
using PrimeTween;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Runtime
{
    public partial class RuntimeBattleCell : MonoBehaviour
    {
        private static uint OutlineRenderingLayerMask => RenderingLayerMask.GetMask(GameplayMetrics.Current.HoveredLayerMaskName);
        public BattleCell BattleCell { get; private set; }
        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }
        public RuntimeHexGrid RuntimeGrid => RuntimeBattleGrid.RuntimeHexGrid;
        public HexCoordinates Coordinates => BattleCell.cell.coordinates;

        [field: SerializeField]
        public SpriteRenderer SpriteRenderer { get; private set; }


        private void OnEnable()
        {
            dragCardPhase = ListPool<PlayerDragBattleCardPhase>.Get();

            this.Register();
        }

        private void OnDisable()
        {
            ListPool<PlayerDragBattleCardPhase>.Release(dragCardPhase);

            this.Unregister();
        }


        public void Connect(RuntimeBattleGrid grid, BattleGameMode currentGameMode, BattleCell battleCell)
        {
            if (BattleCell != null)
                Disconnect(currentGameMode, battleCell);

            RuntimeBattleGrid = grid;
            BattleCell = battleCell;

            Vector3 targetScale = Vector3.one * RuntimeGrid.Current.OuterCellRadius * 1.8f;
            var coordinates = battleCell.cell.coordinates;

            float delay = coordinates.Length() * .08f;
            Tween.Scale(transform, targetScale, new TweenSettings() { startDelay = delay, ease = Ease.OutElastic, duration = 1f});
            transform.localScale = Vector3.zero;
        }

        public void Disconnect(BattleGameMode currentGameMode, BattleCell battleCell)
        {
            if (BattleCell != battleCell)
                return;

            BattleCell = null;
        }
    }
}