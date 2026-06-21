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
    public class DeployPatternController : BattlePatternController
    {
        private readonly IBattlePlayer player;

        public DeployPatternController(BattleGrid battleGrid, IBattlePlayer player) : base(battleGrid)
        {
            this.player = player;
        }


        public override bool Blocks(HexCoordinates c)
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