using System;
using System.Collections.Generic;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.HexGrids.Runtime
{
    [RequireComponent(typeof(RuntimeHexGrid))]
    [RequireComponent(typeof(DeployCardPhaseListener))]
    [RequireComponent(typeof(PlayerTurnPhaseListener))]
    public class RuntimeBattleGrid : MonoBehaviour, IPhaseListener<BattleGameMode>
    {
        [SerializeField]
        private RuntimeHexGrid runtimeHexGrid;

        public BattleGameMode CurrentGameMode { get; private set; }

        private Dictionary<HexCell, RuntimeBattleCell> battleCells;

        private DeployCardPhaseListener deployCardPhaseListener;

        public bool IsInDeployPhase => deployCardPhaseListener != null;

        private List<DeployCardPhase> deployCardPhases = new List<DeployCardPhase>();


        private void Reset()
        {
            runtimeHexGrid = GetComponent<RuntimeHexGrid>();
            deployCardPhaseListener = GetComponent<DeployCardPhaseListener>();
        }

        private void OnEnable()
        {
            battleCells = DictionaryPool<HexCell, RuntimeBattleCell>.Get();
            runtimeHexGrid.OnCellAdded += OnCellAdded;
            runtimeHexGrid.OnCellRemoved += OnCellRemoved;
        }

        private void OnDisable()
        {
            DictionaryPool<HexCell, RuntimeBattleCell>.Release(battleCells);
            runtimeHexGrid.OnCellAdded -= OnCellAdded;
            runtimeHexGrid.OnCellRemoved -= OnCellRemoved;
        }

        void IPhaseListener<BattleGameMode>.OnPhaseBegin(BattleGameMode phase)
        {
            currentGameMode = phase;
            runtimeHexGrid.Connect(currentGameMode.HexGrid);
        }

        void IPhaseListener<BattleGameMode>.OnPhaseEnd(BattleGameMode phase)
        {
            if (currentGameMode == phase)
            {
                currentGameMode = null;
                runtimeHexGrid.Disconnect();
            }
        }


        private void OnCellAdded(RuntimeHexCell runtimeCell)
        {
            RuntimeBattleCell runtimeBattleCell = runtimeCell.gameObject.AddComponent<RuntimeBattleCell>();

            if (currentGameMode.BattleGrid.TryGetBattleCell(runtimeCell.Cell, out BattleCell battleCell))
            {
                runtimeBattleCell.Connect(currentGameMode, battleCell);
                battleCells.Add(runtimeCell.Cell, runtimeBattleCell);
            }
        }

        private void OnCellRemoved(RuntimeHexCell runtimeCell)
        {
            if (battleCells.Remove(runtimeCell.Cell, out RuntimeBattleCell cell))
            {
                if (currentGameMode.BattleGrid.TryGetBattleCell(runtimeCell.Cell, out BattleCell battleCell))
                    cell.Disconnect(currentGameMode, battleCell);

                Destroy(cell);
            }
        }
    }
}