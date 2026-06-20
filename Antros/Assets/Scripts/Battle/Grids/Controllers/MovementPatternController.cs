using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using ATCG.HexGrids.Patterns;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Grids.Controllers
{
    public readonly struct MovementPatternController : IHexPatternController
    {
        public HexGrid HexGrid => battleGrid.grid;
        public readonly BattleGrid battleGrid;
        private readonly HexCoordinates origin;


        public MovementPatternController(BattleGrid battleGrid, HexCoordinates origin)
        {
            this.battleGrid = battleGrid;
            this.origin = origin;
        }


        /// <summary>
        /// True if propagation stops at this coordinate. Branch onto your real
        /// </summary>
        public bool Blocks(HexCoordinates c)
        {
            if (!battleGrid.TryGetBattleCell(c, out var cell))
                return true;

            if (origin == c)
                return false;

            return !cell.CanBeMovedOn();
        }
    }
}