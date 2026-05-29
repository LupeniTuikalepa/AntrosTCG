using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Entities.Runtime.Grid;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Runtime;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Runtime
{
    [RequireComponent(typeof(RuntimeHexGrid))]
    public class RuntimeBattleGrid : MonoBehaviour, IRuntimeBattlePlayerComponent<LocalBattlePlayer>
    {

        public event Action<RuntimeBattleCell> OnBattleCellAdded;
        public event Action<RuntimeBattleCell> OnBattleCellRemoved;

        public RuntimeHexGrid RuntimeHexGrid => runtimeHexGrid;
        public BattleGrid BattleGrid => CurrentBattlePhase?.BattleGrid;
        public BattlePhase CurrentBattlePhase => LocalBattlePlayer.BattlePhase;
        public IReadOnlyCollection<RuntimeBattleCell> BattleCells => battleCells.Values;

        public LocalBattlePlayer LocalBattlePlayer { get; private set; }

        [SerializeField]
        private RuntimeHexGrid runtimeHexGrid;

        [field: SerializeField]
        public RuntimeEntityManager EntityManager { get; private set; }


        private Dictionary<HexCell, RuntimeBattleCell> battleCells;
        private SelectCellPhase lastLookupPhase;



        private void Reset()
        {
            runtimeHexGrid = GetComponent<RuntimeHexGrid>();
        }

        private void OnEnable()
        {
            battleCells = DictionaryPool<HexCell, RuntimeBattleCell>.Get();
            runtimeHexGrid.OnCellAdded += OnGridCellAdded;
            runtimeHexGrid.OnCellRemoved += OnGridCellRemoved;
        }

        private void OnDisable()
        {
            DictionaryPool<HexCell, RuntimeBattleCell>.Release(battleCells);
            runtimeHexGrid.OnCellAdded -= OnGridCellAdded;
            runtimeHexGrid.OnCellRemoved -= OnGridCellRemoved;
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Connect(RuntimeBattlePlayer runtimeBattlePlayer,
            LocalBattlePlayer player)
        {
            LocalBattlePlayer = player;
            runtimeHexGrid.Connect(CurrentBattlePhase.HexGrid);
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer,
            LocalBattlePlayer player)
        {
            runtimeHexGrid.Disconnect();
            LocalBattlePlayer = null;
        }


        private void OnGridCellAdded(RuntimeHexCell runtimeCell)
        {
            RuntimeBattleCell runtimeBattleCell = runtimeCell.GetComponent<RuntimeBattleCell>();

            if (CurrentBattlePhase.BattleGrid.TryGetBattleCell(runtimeCell.Coordinates,
                    out BattleCellAspect battleCell))
            {
                runtimeBattleCell.SetGrid(this);
                _ = runtimeBattleCell.Spawn(EntityManager, battleCell);
                battleCells.Add(runtimeCell.Cell, runtimeBattleCell);
                OnBattleCellAdded?.Invoke(runtimeBattleCell);
            }
        }

        private void OnGridCellRemoved(RuntimeHexCell runtimeCell)
        {
            if (battleCells.Remove(runtimeCell.Cell, out RuntimeBattleCell cell))
            {
                if (CurrentBattlePhase.BattleGrid.TryGetBattleCell(runtimeCell.Coordinates, out BattleCellAspect battleCell))

                OnBattleCellRemoved?.Invoke(cell);
                Destroy(cell);
            }
        }

        public bool TryGetBattleCellAt(HexCoordinates hexCoordinates, out RuntimeBattleCell battleCell)
        {
            if (RuntimeHexGrid.TryGetCellAt(hexCoordinates, out HexCell cell))
                return battleCells.TryGetValue(cell, out battleCell);

            battleCell = null;
            return false;
        }


        public Vector3 GetTargetScale()
        {
            return RuntimeHexGrid.GetTargetScale();
        }

    }
}