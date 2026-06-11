using System.Collections.Generic;
using ATCG.Battle.Cards;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
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


        public World World => battlePhase.world;

        public IEnumerable<HexCoordinates> AllCellsCoordinates => battleCellsEntities.Keys;


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
                BattleCellAspect cellAspect = BattleCellAspect.CreateAspect(World, new BattleCellAspect.Setup()
                {
                    coordinates = coordinate,
                    battleGrid = this
                });

                battleCellsEntities.Add(coordinate, cellAspect);
            }
        }
        public bool TryGetBattleCell(HexCoordinates coordinates, out BattleCellAspect cellAspect)
        {
            return battleCellsEntities.TryGetValue(coordinates, out cellAspect);
        }

        public EntityQueryResult GetGridMembers()
        {
            return World.Query(EntityQuery.With<BattleGridElementComponent>());
        }

        public void FillDeployableCells(List<HexCoordinates> list)
        {
            list.AddRange(AllCellsCoordinates);

            foreach (Entity entity in World.Query(EntityQuery.With<PhysicalCellMemberTag>()))
            {
                if (entity.TryGetROComponent(World, out BattleGridElementComponent gridEntityComponent))
                {
                    Debug.Log($"Removing {gridEntityComponent.coordinates}");
                    list.Remove(gridEntityComponent.coordinates);
                }
            }
        }

        public static implicit operator HexGrid(BattleGrid battleGrid) => battleGrid.grid;
    }
}