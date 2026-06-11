using System;
using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public readonly struct SpecificBattleCellFilter : IEntityFilter
    {
        private readonly HashSet<HexCoordinates> validCells;

        public SpecificBattleCellFilter(HashSet<HexCoordinates> validCells)
        {
            this.validCells = validCells;
        }

        public bool Accepts(EntityAddress address)
        {
            if (address.IsBattleCellAspect(out BattleCellAspect battleCell))
                return validCells.Contains(battleCell.Coordinate);

            return false;
        }
    }
}