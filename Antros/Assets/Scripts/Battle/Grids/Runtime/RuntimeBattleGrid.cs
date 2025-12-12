using System;
using System.Collections.Generic;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Runtime
{
    [RequireComponent(typeof(RuntimeHexGrid))]
    public class RuntimeBattleGrid : MonoBehaviour, IPhaseListener<BattleGameMode>
    {
        public event Action<RuntimeBattleCell> OnBattleCellAdded;
        public event Action<RuntimeBattleCell> OnBattleCellRemoved;

        [SerializeField]
        private RuntimeHexGrid runtimeHexGrid;

        public RuntimeHexGrid RuntimeHexGrid => runtimeHexGrid;

        public BattleGrid BattleGrid => CurrentGameMode?.BattleGrid;

        public BattleGameMode CurrentGameMode { get; private set; }

        private Dictionary<HexCell, RuntimeBattleCell> battleCells;
        public IReadOnlyCollection<RuntimeBattleCell> BattleCells => battleCells.Values;

        private void Reset()
        {
            runtimeHexGrid = GetComponent<RuntimeHexGrid>();
        }

        private void OnEnable()
        {
            battleCells = DictionaryPool<HexCell, RuntimeBattleCell>.Get();
            runtimeHexGrid.OnCellAdded += OnGridCellAdded;
            runtimeHexGrid.OnCellRemoved += OnGridCellRemoved;

            this.Register();
        }

        private void OnDisable()
        {
            DictionaryPool<HexCell, RuntimeBattleCell>.Release(battleCells);
            runtimeHexGrid.OnCellAdded -= OnGridCellAdded;
            runtimeHexGrid.OnCellRemoved -= OnGridCellRemoved;

            this.Unregister();
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

        private void OnGridCellAdded(RuntimeHexCell runtimeCell)
        {
            RuntimeBattleCell runtimeBattleCell = runtimeCell.GetComponent<RuntimeBattleCell>();

            if (CurrentGameMode.BattleGrid.TryGetBattleCell(runtimeCell.Cell, out BattleCell battleCell))
            {
                runtimeBattleCell.Connect(this, CurrentGameMode, battleCell);
                battleCells.Add(runtimeCell.Cell, runtimeBattleCell);
                OnBattleCellAdded?.Invoke(runtimeBattleCell);
            }
        }

        private void OnGridCellRemoved(RuntimeHexCell runtimeCell)
        {
            if (battleCells.Remove(runtimeCell.Cell, out RuntimeBattleCell cell))
            {
                if (CurrentGameMode.BattleGrid.TryGetBattleCell(runtimeCell.Cell, out BattleCell battleCell))
                    cell.Disconnect(CurrentGameMode, battleCell);

                OnBattleCellRemoved?.Invoke(cell);
                Destroy(cell);
            }
        }
    }
}