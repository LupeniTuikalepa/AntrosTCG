using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Components.Tags;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using ATCG.HexGrids.Patterns;
using UnityEngine;

namespace ATCG.Battle.Grids.Controllers
{
    public readonly struct DeployPatternController : IHexPatternController
    {
        public HexGrid HexGrid => battleGrid.grid;
        public readonly BattleGrid battleGrid;

        private readonly IBattlePlayer player;

        public DeployPatternController(BattleGrid battleGrid, IBattlePlayer player)
        {
            this.battleGrid = battleGrid;
            this.player = player;
        }


        /// <summary>
        /// True if propagation stops at this coordinate. Branch onto your real
        /// BattleGrid blocking method (wall / occupied / off-grid).
        /// </summary>
        public bool Blocks(HexCoordinates c)
        {
            if (!battleGrid.TryGetBattleCell(c, out var cell))
                return true;

            foreach (var member in cell.GetPhysicalMembers())
            {
                if (member.EntityAddress.TryGetComponentRO(out BelongsToPlayerComponent belongsToPlayer))
                    return !belongsToPlayer.IsAllieOf(player);

                return true;
            }

            return false;
        }
    }
}