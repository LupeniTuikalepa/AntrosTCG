using System;
using System.Linq;
using UnityEngine.Pool;

namespace ATCG.Battle.Cards.Capacities.Interfaces
{
    [Flags]
    public enum CapacityTargetType
    {
        Nothing = 0,
        Everything = -1,
        Self = 1 << 0,
        Opponents = 1 << 1,
        Allies = 1 << 2,
        Cells = 1 << 3,
    }
}