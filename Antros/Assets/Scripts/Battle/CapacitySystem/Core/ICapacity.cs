using System.Collections.Generic;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Aspects;
using ATCG.Battle.Grids;
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
        void GetTargets(T data, BattleCellAspect battleCell, List<EntityAddress> output)
        {
            output.Add(battleCell.EntityAddress);
            foreach (var member in battleCell.GetMembers())
                output.Add(member.EntityAddress);
        }

        [AddToContainer]
        HexPatternBuilder GetHitPattern(T data, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin);

        [AddToContainer]
        ICapacityStep[] GetSteps(T data, CastCapacityPhase phase);
    }
}