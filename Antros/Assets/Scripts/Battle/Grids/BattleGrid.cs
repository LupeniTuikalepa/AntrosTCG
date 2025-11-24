using System;
using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using ATCG.HexGrids.Shapes;
using Helteix.Cards.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.HexGrids
{
    public class BattleGrid : IDisposable
    {
        public event Action<BattleCell> OnBattleCellAdded;
        public event Action<BattleCell> OnBattleCellRemoved;

        public event Action<IBattleCard> OnBattleCardDeployed;
        public event Action<IBattleCard> OnBattleCardLeft;
        public HexGrid Grid { get; }

        private DefaultCardCollection<IBattleCard> cards;

        private Dictionary<IBattleCard, HexCoordinates> cardCoordinates;

        private Dictionary<HexCell, BattleCell> battleCells;

        public BattleGrid(uint cellRadius, uint gridRadius)
        {
            Grid = new HexGrid(cellRadius, Vector2.zero);
            Grid.OnCellAdded += CreateBattleCell;
            Grid.OnCellRemoved += DestroyBattleCell;

            HexagonalShapeBuilder shapeBuilder = new HexagonalShapeBuilder(gridRadius);
            shapeBuilder.Build(Grid);

            cardCoordinates = DictionaryPool<IBattleCard, HexCoordinates>.Get();
            battleCells = DictionaryPool<HexCell, BattleCell>.Get();
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

        public void DeployCard(IBattleCard card, HexCoordinates coordinates)
        {
            if (CanDeploy(coordinates) && cards.TryAddCard(card))
            {
                cardCoordinates.Add(card, coordinates);
                card.Deploy(this, coordinates);
                if(Grid.TryGetCell(coordinates, out HexCell cell))
                    cell.SetMember(card);

                OnBattleCardDeployed?.Invoke(card);
            }
        }

        public void RemoveCard(IBattleCard card)
        {
            if (cards.TryRemoveCard(card) && cardCoordinates.TryGetValue(card, out HexCoordinates coordinates))
            {
                if(Grid.TryGetCell(coordinates, out HexCell cell) && cell.CurrentMember == card)
                    cell.SetMember(null);

                card.Leave();
                OnBattleCardLeft?.Invoke(card);
            }
        }
        public bool MoveCardTo(IBattleCard card, HexCoordinates destination)
        {
            if (!cardCoordinates.TryGetValue(card, out HexCoordinates lastCoordinates))
                return false;

            if (lastCoordinates == destination)
                return false;

            if (HasMember(destination))
                return false;

            cardCoordinates[card] = destination;
            card.OnCardMoved(lastCoordinates, destination);
            return true;
        }

        public bool TryGetBattleCell(HexCell hexCell, out BattleCell cell) =>
            battleCells.TryGetValue(hexCell, out cell);

        public bool TryGetBattleCell(HexCoordinates coordinates, out BattleCell cell)
        {
            if (Grid.TryGetCell(coordinates, out HexCell hexCell))
                return TryGetBattleCell(hexCell, out cell);

            cell = null;
            return false;
        }

        public HexCoordinates GetCardCoordinates(IBattleCard battleCard) =>
            TryGetCardCoordinates(battleCard, out HexCoordinates c) ? c : HexCoordinates.None;

        public bool TryGetCardCoordinates(IBattleCard battleCard, out HexCoordinates coordinates) =>
            cardCoordinates.TryGetValue(battleCard, out coordinates);



        public IEnumerable<IHexMember> GetAllMembers()
            => Grid.GetMembers();

        public bool TryGetMembers(HexCoordinates coord, out IHexMember member)
            => Grid.TryGetHexMember(coord, out member);

        public bool CanDeploy(HexCoordinates coordinates) => Grid.HasMember(coordinates);
        public bool HasMember(HexCoordinates coordinates) => Grid.HasMember(coordinates);

        void IDisposable.Dispose()
        {
            DictionaryPool<IBattleCard, HexCoordinates>.Release(cardCoordinates);
            DictionaryPool<HexCell, BattleCell>.Release(battleCells);
        }

    }
}