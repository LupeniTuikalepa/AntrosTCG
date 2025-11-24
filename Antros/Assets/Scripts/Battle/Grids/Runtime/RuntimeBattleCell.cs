using System;
using ATCG.Battle.Cards;
using ATCG.Battle.Metrics;
using ATCG.Battle.Players;
using Helteix.Cards.UI.Physical.Drag;
using PrimeTween;
using UnityEngine;

namespace ATCG.Battle.HexGrids.Runtime
{
    public partial class RuntimeBattleCell : MonoBehaviour
    {

        private static uint OutlineRenderingLayerMask =>
            RenderingLayerMask.GetMask(GameplayMetrics.Current.HoveredLayerMaskName);

        [SerializeField]
        private MeshRenderer meshRenderer;

        public BattleCell Cell { get; private set; }
        public RuntimeBattleGrid BattleGrid { get; private set; }


        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void SetGrid(RuntimeBattleGrid grid)
        {
            BattleGrid = grid;
        }

        public void Connect(BattleGameMode currentGameMode, BattleCell battleCell)
        {
            if (Cell != null)
                Disconnect(currentGameMode, battleCell);
            Cell = battleCell;
        }

        public void Disconnect(BattleGameMode currentGameMode, BattleCell battleCell)
        {
            if (Cell != battleCell)
                return;

            Cell = null;
        }
    }
}