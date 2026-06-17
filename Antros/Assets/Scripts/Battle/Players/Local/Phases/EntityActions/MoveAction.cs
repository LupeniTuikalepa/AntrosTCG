using System;
using System.Collections.Generic;
using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Cards.Capacities.Behaviours.Mapping;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
using ATCG.Battle.Commands.GameCommands.Players;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.GameModes;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Patterns;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Battle.Players;
using ATCG.Battle.Players.Local;
using ATCG.Battle.Players.Local.Phases;
using ATCG.HexGrids;
using ATCG.Metrics;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle
{
    public class MoveAction : EntityAction
    { 
        public override int ManaCost => GameMetrics.Current.MovementCost;

        private readonly int speed;
        public MoveAction(LocalBattlePlayer player, int speed) : base(player)
        {
            this.speed = speed;
        }
        
        public override async Awaitable Execute(EntityAddress address, BattlePhase battlePhase)
        {
            if (!address.TryGetComponentRO(out GridMemberComponent gridMemberComponent))
                return;
            
            if(!address.TryGetComponentRO(out MovementComponent movementComponent))
                return;
            
            HexCoordinates center = gridMemberComponent.coordinates;
            var movementPatternData = movementComponent.patternDatas;
            
            var pathPhase = new CreatePathPhase(playerOrigin, center, speed, movementPatternData);
            HexCoordinates[] result = await pathPhase.Run();
            if (result.Length == 0)
                return;

            var manaCommand = new ModifyPlayerManaCommand(playerOrigin.ID, -ManaCost);
            manaCommand.Run(battlePhase);
            
            var pathCommand = new MovePathCommand(address, result);
            await pathCommand.RunAsync(battlePhase);
            
        }
    }
}
