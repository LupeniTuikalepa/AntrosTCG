using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;
using ATCG.Battle.Grids;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using Helteix.Tools.Phases;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public class CreatePathPhase : LocalPlayerPhase<HexCoordinates[]>
    {
        public event Action<IEnumerable<HexCoordinates>> OnPathChanged;
        
        public readonly HexCoordinates startingPoint;
        
        private readonly int speed;
        private readonly CapacityPatternData[] patternData;
        
        private readonly struct GridFilter : IEntityFilter
        {
            public bool Accepts(EntityAddress entityAddress)
            {
                return entityAddress.Is<BattleCellAspect>(out var cell) && cell.CanBeMovedOn();
            }
        }
        
        public CreatePathPhase(LocalBattlePlayer localBattlePlayer,HexCoordinates startingPoint , int speed, CapacityPatternData[] patternData) : base(localBattlePlayer)
        {
            this.startingPoint = startingPoint;
            this.speed = speed;
            this.patternData = patternData;
        }

        protected override async Awaitable<HexCoordinates[]> Execute(CancellationToken token)
        {
            using (ListPool<HexCoordinates>.Get(out var list))
            {
                var filter = new GridFilter();
                var center  = startingPoint;
                for (int i = 0; i < speed; i++)
                {
                    using HexPatternBuilder builder = patternData
                        .ToPatternBuilder(center)
                        .Without(center);
                    
                    EntityAddress[] result = await new SelectEntityPhase<GridFilter>(LocalBattlePlayer, filter, builder);
                    
                    if (result.Length <= 0)
                        return Array.Empty<HexCoordinates>();
                    
                    for (int j = 0; j < result.Length; j++)
                    {
                        var selectedCell = result[j];
                        if (!selectedCell.TryGetComponentRO(out GridMemberComponent cellComponent))
                            continue;
                        list.Add(cellComponent.coordinates);
                    }
                    center = list[^1];
                    OnPathChanged?.Invoke(list);
                }
                return list.ToArray();
            }
        }
    }
}