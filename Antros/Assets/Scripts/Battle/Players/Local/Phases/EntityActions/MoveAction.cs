using System;
using System.Collections.Generic;
using ATCG.Battle.Cards.Capacities;
using ATCG.Battle.Cards.Capacities.Behaviours.Mapping;
using ATCG.Battle.Commands.Core;
using ATCG.Battle.Commands.EntityCommands;
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
        private readonly struct GridFilter : IEntityFilter
         {
             public bool Accepts(EntityAddress entityAddress)
             {
                 return entityAddress.Is<BattleCellAspect>(out var cell) && cell.CanBeMovedOn();
             }
         }
        
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
            var movementComponentPatternData = movementComponent.patternDatas;
            
            
            var filter = new GridFilter();
            using (ListPool<HexCoordinates>.Get(out var list))
            {
                for (int i = 0; i < speed; i++)
                {
                    using HexPatternBuilder builder = movementComponentPatternData
                        .ToPatternBuilder(center)
                        .Without(center);
                    
                    EntityAddress[] result = await new SelectEntityPhase<GridFilter>(playerOrigin, filter, builder);
                    
                    for (int j = 0; j < result.Length; j++)
                    {
                        var selectedCell = result[j];
                        if (!selectedCell.TryGetComponentRO(out GridMemberComponent cellComponent))
                            return;
                        list.Add(cellComponent.coordinates);
                    }
                    center = list[^1];
                }
                var pathCommand = new MovePathCommand(address, list);
                await pathCommand.RunAsync(battlePhase);
            } 
        }
    }
}
