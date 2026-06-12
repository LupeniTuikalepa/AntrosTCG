using System;
using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public readonly struct HexPatternFilters : IEntityFilter
    {
        private readonly HexPatternBuilder patternBuilder;

        public HexPatternFilters(HexPatternBuilder patternBuilder)
        {
            this.patternBuilder = patternBuilder;
        }

        public bool Accepts(EntityAddress address) =>
            address.IsBattleCellAspect(out BattleCellAspect battleCell)
            && patternBuilder.Contains(battleCell.Coordinate);
    }
}