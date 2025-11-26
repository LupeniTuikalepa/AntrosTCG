using System;
using ATCG.Battle.Metrics;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.HexGrids.Runtime
{
    public partial class RuntimeBattleCell : MonoBehaviour
    {
        private static uint OutlineRenderingLayerMask => RenderingLayerMask.GetMask(GameplayMetrics.Current.HoveredLayerMaskName);

        [SerializeField]
        private MeshRenderer meshRenderer;

        public BattleCell BattleCell { get; private set; }
        public RuntimeBattleGrid RuntimeBattleGrid { get; private set; }

        public HexCoordinates Coordinates => BattleCell.cell.coordinates;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

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


        private void SetGrid(RuntimeBattleGrid grid)
        {
            RuntimeBattleGrid = grid;
        }

        public void Connect(BattleGameMode currentGameMode, BattleCell battleCell)
        {
            if (BattleCell != null)
                Disconnect(currentGameMode, battleCell);
            BattleCell = battleCell;
        }

        public void Disconnect(BattleGameMode currentGameMode, BattleCell battleCell)
        {
            if (BattleCell != battleCell)
                return;

            BattleCell = null;
        }
    }
}