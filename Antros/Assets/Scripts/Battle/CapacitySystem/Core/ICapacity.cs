using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Grids;
using ATCG.Capacities;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    [GenerateContainer]
    public interface ICapacity<in T> : IBehaviour<T> where T : CapacityData
    {
        [AddToContainer]
        HexPatternBuilder GetHitPattern(T data, BattleGrid battleGrid, HexCoordinates origin);

        [AddToContainer]
        IEnumerable<ICapacityStep> Run(T data, CastCapacityPhase phase);
    }
}