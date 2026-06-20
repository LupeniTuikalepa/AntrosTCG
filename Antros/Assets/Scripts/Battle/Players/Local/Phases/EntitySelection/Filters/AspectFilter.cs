using System;
using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Entities.Queries;
using ATCG.HexGrids;
using UnityEngine.Pool;

namespace ATCG.Battle.Players.Local.Phases
{
    public readonly struct AspectFilter<T> : IEntityFilter where T : IEntityAspect
    {
        public bool Accepts(EntityAddress address) => address.Is<BattleCellAspect>();
    }
}