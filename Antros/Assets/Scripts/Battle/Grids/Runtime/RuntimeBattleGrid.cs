using System;
using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Heroes.Runtime;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local.Phases;
using ATCG.Battle.Players.Runtime;
using ATCG.HexGrids;
using ATCG.HexGrids.Runtime;
using ATCG.Metrics;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Runtime
{
    [RequireComponent(typeof(RuntimeHexGrid))]
    public class RuntimeBattleGrid : MonoBehaviour, IRuntimeBattlePlayerComponent<LocalBattlePlayer>,
        IPhaseListener<SelectCellPhase>
    {
        public event Action<SelectCellPhase> OnNewLookupPhaseActivated;
        public event Action<RuntimeBattleCell> OnBattleCellAdded;
        public event Action<RuntimeBattleCell> OnBattleCellRemoved;
        public event Action<RuntimeHero> OnRuntimeHeroCreated;
        public event Action<RuntimeHero> OnRuntimeHeroDestroyed;

        [SerializeField]
        private RuntimeHexGrid runtimeHexGrid;

        [SerializeField]
        private Transform heroContainer;

        private Dictionary<HexCell, RuntimeBattleCell> battleCells;


        public RuntimeHexGrid RuntimeHexGrid => runtimeHexGrid;
        public BattleGrid BattleGrid => CurrentBattlePhase?.BattleGrid;
        public BattlePhase CurrentBattlePhase => LocalBattlePlayer.BattlePhase;
        public IReadOnlyCollection<RuntimeBattleCell> BattleCells => battleCells.Values;

        private Dictionary<HeroBattleCard, RuntimeHero> heroCards = new();

        private List<SelectCellPhase> lookupPhases = new();
        public SelectCellPhase CurrentLookupPhase => lookupPhases.Count > 0 ? lookupPhases[0] : null;

        private SelectCellPhase lastLookupPhase;

        public RuntimeHero SelectedCard { get; private set; }
        public LocalBattlePlayer LocalBattlePlayer { get; private set; }


        private void Reset()
        {
            runtimeHexGrid = GetComponent<RuntimeHexGrid>();
        }


        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Connect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer player)
        {
            LocalBattlePlayer = player;
            BattleGrid.OnBattleCardDeployed += OnCardDeployed;
            runtimeHexGrid.Connect(CurrentBattlePhase.HexGrid);
        }

        void IRuntimeBattlePlayerComponent<LocalBattlePlayer>.Disconnect(RuntimeBattlePlayer runtimeBattlePlayer, LocalBattlePlayer battlePlayer)
        {
            BattleGrid.OnBattleCardDeployed -= OnCardDeployed;
            runtimeHexGrid.Disconnect();

            LocalBattlePlayer = null;
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



        private void OnGridCellAdded(RuntimeHexCell runtimeCell)
        {
            RuntimeBattleCell runtimeBattleCell = runtimeCell.GetComponent<RuntimeBattleCell>();

            if (CurrentBattlePhase.BattleGrid.TryGetBattleCell(runtimeCell.Cell, out BattleCell battleCell))
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
                if (CurrentBattlePhase.BattleGrid.TryGetBattleCell(runtimeCell.Cell, out BattleCell battleCell))
                    cell.Disconnect(CurrentBattlePhase, battleCell);

                OnBattleCellRemoved?.Invoke(cell);
                Destroy(cell);
            }
        }

        private void OnCardDeployed(IBattleCard card)
        {
            switch (card)
            {
                case HeroBattleCard heroBattleCard:
                    if(heroCards.ContainsKey(heroBattleCard))
                        break;

                    GameObject instance = GameAssets.Current.HeroPawnPrefab.InstantiatePrefab(heroContainer);
                    if (instance.TryGetComponent(out RuntimeHero runtimeHeroBattleCard))
                    {
                        runtimeHeroBattleCard.Initialize(this);
                        runtimeHeroBattleCard.Connect(heroBattleCard);

                        heroCards.Add(heroBattleCard, runtimeHeroBattleCard);
                        OnRuntimeHeroCreated?.Invoke(runtimeHeroBattleCard);
                    }

                    break;
            }
        }

        public bool TryGetBattleCellAt(HexCoordinates hexCoordinates, out RuntimeBattleCell battleCell)
        {
            if (RuntimeHexGrid.TryGetCellAt(hexCoordinates, out var cell))
                return battleCells.TryGetValue(cell, out battleCell);

            battleCell = null;
            return false;
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
    }
}