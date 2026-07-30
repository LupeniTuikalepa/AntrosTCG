using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
using ATCG.Battle.Players;
using ATCG.Capacities;
using ATCG.HexGrids;
using ATCG.HexGrids.Patterns.Building;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Core
{

    [GenerateContainer]
    public interface ICapacity<in T> : IBehaviour<T> where T : CapacityData
    {
        [AddToContainer]
        void GetHitPattern(T data, ref HexPatternBuilder builder, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin);

        [AddToContainer]
        void GetTargets(T data, BattleCellAspect battleCell, CapacityTargets output, IBattlePlayer castingPlayer);

        [AddToContainer]
        ICapacityStep[] GetSteps(T data, CastCapacityPhase phase);
    }
}