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
    public class RuntimeBattleGrid : MonoBehaviour, IPhaseListener<BattleGameMode>
    {
        [SerializeField]
        private RuntimeHexGrid runtimeHexGrid;


        public BattleGrid BattleGrid => CurrentGameMode?.BattleGrid;

        public BattleGameMode CurrentGameMode { get; private set; }

        private Dictionary<HexCell, RuntimeBattleCell> battleCells;


        private void Reset()
        {
            runtimeHexGrid = GetComponent<RuntimeHexGrid>();
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
            CurrentGameMode = phase;
            runtimeHexGrid.Connect(CurrentGameMode.HexGrid);
        }

        void IPhaseListener<BattleGameMode>.OnPhaseEnd(BattleGameMode phase)
        {
            if (CurrentGameMode == phase)
            {
                CurrentGameMode = null;
                runtimeHexGrid.Disconnect();
            }
        }

        private void OnCellAdded(RuntimeHexCell runtimeCell)
        {
            RuntimeBattleCell runtimeBattleCell = runtimeCell.gameObject.AddComponent<RuntimeBattleCell>();

            if (CurrentGameMode.BattleGrid.TryGetBattleCell(runtimeCell.Cell, out BattleCell battleCell))
            {
                runtimeBattleCell.Connect(CurrentGameMode, battleCell);
                battleCells.Add(runtimeCell.Cell, runtimeBattleCell);
            }
        }

        private void OnCellRemoved(RuntimeHexCell runtimeCell)
        {
            if (battleCells.Remove(runtimeCell.Cell, out RuntimeBattleCell cell))
            {
                if (CurrentGameMode.BattleGrid.TryGetBattleCell(runtimeCell.Cell, out BattleCell battleCell))
                    cell.Disconnect(CurrentGameMode, battleCell);

                Destroy(cell);
            }
        }
    }
}