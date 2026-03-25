using System;
using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Players;
using ATCG.Cards;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using ATCG.HexGrids.Shapes;
using Helteix.Cards.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids
{
    public class BattleGrid : IDisposable
    {
        public event Action<BattleCell> OnBattleCellAdded;
        public event Action<BattleCell> OnBattleCellRemoved;

        public event Action<IBattleCard> OnBattleCardDeployed;
        public event Action<IBattleCard> OnBattleCardLeft;
        public HexGrid Grid { get; }

        private DefaultCardCollection<IBattleCard> cards;

        private readonly Dictionary<HexCell, BattleCell> battleCells;

        public BattleGrid(uint cellRadius, uint gridRadius)
        {
            Grid = new HexGrid(cellRadius, Vector2.zero);
            Grid.OnCellAdded += CreateBattleCell;
            Grid.OnCellRemoved += DestroyBattleCell;
            cards = new DefaultCardCollection<IBattleCard>();

            battleCells = DictionaryPool<HexCell, BattleCell>.Get();

            HexagonalShapeBuilder shapeBuilder = new HexagonalShapeBuilder(gridRadius);
            shapeBuilder.Build(Grid);
        }

        private void CreateBattleCell(HexCell cell)
        {
            BattleCell battleCell = new BattleCell(this, cell);
            battleCells.Add(cell, battleCell);

            OnBattleCellAdded?.Invoke(battleCell);
        }

        private void DestroyBattleCell(HexCell cell)
        {
            if (battleCells.Remove(cell, out BattleCell value))
            {
                OnBattleCellRemoved?.Invoke(value);
            }
        }
/*
        public void DeployCard(IBattleCard card, HexCoordinates coordinates)
        {
            if (CanDeploy(card.Player, coordinates) && cards.TryAddCard(card))
            {
                card.Deploy(this, coordinates);
                if(Grid.TryGetCell(coordinates, out HexCell cell))
                    cell.AddMember(card);

                OnBattleCardDeployed?.Invoke(card);
            }
        }


        public void RemoveCard(IBattleCard card)
        {
            if (cards.TryRemoveCard(card))
            {
                if(Grid.TryGetCell(card.Coordinates, out HexCell cell))
                    cell.RemoveMember(null);

                card.Leave();

                OnBattleCardLeft?.Invoke(card);
            }
        }
        */

        public IEnumerable<BattleCell> GetCells(Func<BattleCell, bool> filter)
        {
            foreach (var cell in GetCells())
            {
                if(filter(cell))
                    yield return cell;
            }
        }

        public IEnumerable<BattleCell> GetCells() => battleCells.Values;


        public BattleCell GetBattleCell(HexCoordinates coordinates) => TryGetBattleCell(coordinates, out BattleCell cell) ?
            cell :
            null;
        public bool TryGetBattleCell(HexCell hexCell, out BattleCell cell) =>
            battleCells.TryGetValue(hexCell, out cell);

        public bool TryGetBattleCell(HexCoordinates coordinates, out BattleCell cell)
        {
            if (Grid.TryGetCell(coordinates, out HexCell hexCell))
                return TryGetBattleCell(hexCell, out cell);

            cell = null;
            return false;
        }

        void IDisposable.Dispose()
        {
            DictionaryPool<HexCell, BattleCell>.Release(battleCells);
        }

    }
}