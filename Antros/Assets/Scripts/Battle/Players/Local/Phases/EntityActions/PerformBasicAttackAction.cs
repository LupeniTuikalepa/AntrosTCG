using System.Collections.Generic;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Runtime.Heroes;
using ATCG.Battle.GameModes;
using ATCG.Battle.Players;
using ATCG.HexGrids;
using ATCG.HexGrids.Grids;
using ATCG.HexGrids.Utility;
using ATCG.Metrics;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class PerformBasicAttackAction : IEntityAction
    {
        private readonly int strength;
        public int ManaCost => GameMetrics.Current.BasicAttackCost;
        public PerformBasicAttackAction(int strength)
        {
            this.strength = strength;
        }
        
        public async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
	        var command = new PhysicalAttackCommand(address, strength);
	        await command.Run(battlePhase);
        }
        
    }
}