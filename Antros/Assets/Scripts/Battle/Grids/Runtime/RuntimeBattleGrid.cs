using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Heroes.Runtime;
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
    public class RuntimeBattleGrid : MonoBehaviour, IRuntimeBattlePlayerComponent<LocalBattlePlayer>,
        IPhaseListener<SelectCellPhase>
    {
        [SerializeField]
        private RuntimeHexGrid runtimeHexGrid;

        [SerializeField]
        private RuntimeHeroManager heroManager;

        private readonly List<SelectCellPhase> lookupPhases = new();

        private Dictionary<HexCell, RuntimeBattleCell> battleCells;
        private SelectCellPhase lastLookupPhase;

        public RuntimeHexGrid RuntimeHexGrid => runtimeHexGrid;
        public BattleGrid BattleGrid => CurrentBattlePhase?.BattleGrid;
        public BattlePhase CurrentBattlePhase => LocalBattlePlayer.BattlePhase;
        public IReadOnlyCollection<RuntimeBattleCell> BattleCells => battleCells.Values;
        public SelectCellPhase CurrentLookupPhase => lookupPhases.Count > 0 ? lookupPhases[0] : null;

        public LocalBattlePlayer LocalBattlePlayer { get; private set; }


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


        void IPhaseListener<SelectCellPhase>.OnPhaseBegin(SelectCellPhase phase)
        {
            if (phase.Player == LocalBattlePlayer)
            {
                lookupPhases.Add(phase);
                RefreshLookupPhase();
            }
        }

        void IPhaseListener<SelectCellPhase>.OnPhaseEnd(SelectCellPhase phase)
        {
            if (phase.Player == LocalBattlePlayer)
            {
                lookupPhases.Remove(phase);
                RefreshLookupPhase();
            }
        }


        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Connect(RuntimeBattlePlayer runtimeBattlePlayer,
            LocalBattlePlayer player)
        {
            LocalBattlePlayer = player;
            runtimeHexGrid.Connect(CurrentBattlePhase.HexGrid);
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer,
            LocalBattlePlayer battlePlayer)
        {
            runtimeHexGrid.Disconnect();
            LocalBattlePlayer = null;
        }

        public event Action<SelectCellPhase> OnNewLookupPhaseActivated;
        public event Action<RuntimeBattleCell> OnBattleCellAdded;
        public event Action<RuntimeBattleCell> OnBattleCellRemoved;


        private void OnGridCellAdded(RuntimeHexCell runtimeCell)
        {
            RuntimeBattleCell runtimeBattleCell = runtimeCell.GetComponent<RuntimeBattleCell>();

            if (CurrentBattlePhase.BattleGrid.TryGetBattleCell(runtimeCell.Coordinates,
                    out BattleCellAspect battleCell))
            {
                runtimeBattleCell.Connect(this, CurrentBattlePhase, battleCell);
                battleCells.Add(runtimeCell.Cell, runtimeBattleCell);
                OnBattleCellAdded?.Invoke(runtimeBattleCell);
            }
        }

        private void OnGridCellRemoved(RuntimeHexCell runtimeCell)
        {
            if (battleCells.Remove(runtimeCell.Cell, out RuntimeBattleCell cell))
            {
                if (CurrentBattlePhase.BattleGrid.TryGetBattleCell(runtimeCell.Coordinates,
                        out BattleCellAspect battleCell))
                    cell.Disconnect(CurrentBattlePhase, battleCell);

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

        private void RefreshLookupPhase()
        {
            SelectCellPhase newCurrentPhase = CurrentLookupPhase;
            if (lastLookupPhase == newCurrentPhase)
                return;

            foreach ((HexCell hexCell, RuntimeBattleCell runtimeBattleCell) in battleCells)
                runtimeBattleCell.RefreshLookupPhaseStatus(lastLookupPhase, newCurrentPhase);

            OnNewLookupPhaseActivated?.Invoke(newCurrentPhase);
            lastLookupPhase = newCurrentPhase;
        }

        public Vector3 GetTargetScale()
        {
            return RuntimeHexGrid.GetTargetScale();
        }
    }
}