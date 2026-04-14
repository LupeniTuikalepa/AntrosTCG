using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.GameModes;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using ATCG.HexGrids.Shapes;
using Helteix.Cards.Collections;
using UnityEngine;

namespace ATCG.Battle.Grids
{
    public class BattleGrid
    {
        private readonly Dictionary<HexCoordinates, BattleCellAspect> battleCellsEntities;

        public readonly BattlePhase battlePhase;
        public readonly HexGrid grid;

        private DefaultCardCollection<IBattleCard> cards;

        public BattleGrid(BattlePhase battlePhase, uint cellRadius, uint gridRadius)
        {
            this.battlePhase = battlePhase;
            grid = new HexGrid(cellRadius, Vector2.zero);

            HexagonalShapeBuilder shapeBuilder = new(gridRadius);
            shapeBuilder.Build(grid);

            cards = new DefaultCardCollection<IBattleCard>();
            battleCellsEntities = new Dictionary<HexCoordinates, BattleCellAspect>();

            foreach (HexCoordinates coordinate in grid.CellsCoordinates)
            {
                EntityAspectBuilder<BattleCellAspect> builder = new EntityAspectBuilder<BattleCellAspect>()
                {
                    new ComponentFactory<BattleCellComponent>(() => new BattleCellComponent(coordinate))
                };

                BattleCellAspect cellAspect = builder.CreateAndDispose(World);
                battleCellsEntities.Add(coordinate, cellAspect);
            }
        }

        public World World => battlePhase.world;

        public IEnumerable<HexCoordinates> AllCellsCoordinates => battleCellsEntities.Keys;

        public bool TryGetBattleCell(HexCoordinates coordinates, out BattleCellAspect cellAspect)
        {
            return battleCellsEntities.TryGetValue(coordinates, out cellAspect);
        }

        public EntityQueryResult GetGridMembers()
        {
            return World.Query(Query.With<GridMemberComponent>());
        }

        public bool TryGetCellFor(HexCoordinates coordinates, out BattleCellAspect battleCellAspect)
        {
            return battleCellsEntities.TryGetValue(coordinates, out battleCellAspect);
        }

        /*
        public void DeployCard(IBattleCard HeroCard, HexCoordinates coordinates)
        {
            if (CanDeploy(HeroCard.Player, coordinates) && cards.TryAddCard(HeroCard))
            {
                HeroCard.Deploy(this, coordinates);
                if(Grid.TryGetCell(coordinates, out HexCell cell))
                    cell.AddMember(HeroCard);

                OnBattleCardDeployed?.Invoke(HeroCard);
            }
        }


        public void RemoveCard(IBattleCard HeroCard)
        {
            if (cards.TryRemoveCard(HeroCard))
            {
                if(Grid.TryGetCell(HeroCard.AllCellsCoordinates, out HexCell cell))
                    cell.RemoveMember(null);

                HeroCard.Leave();

                OnBattleCardLeft?.Invoke(HeroCard);
            }
        }

        public IEnumerable<BattleCell> GetCells() => battleCells.Values;

        public IEnumerable<BattleCell> GetCells(Func<BattleCell, bool> filter)
        {
            foreach (var cell in GetCells())
            {
                if(filter(cell))
                    yield return cell;
            }
        }

        public BattleCell GetBattleCell(HexCoordinates coordinates) => TryGetBattleCell(coordinates, out BattleCell cell) ? cell : default;
        */
    }
}