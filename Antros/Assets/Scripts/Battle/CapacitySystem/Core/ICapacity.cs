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
        HexPatternBuilder GetHitPattern(T data, BattleGrid battleGrid, HexCoordinates castPoint, HexCoordinates casterOrigin);

        [AddToContainer]
        ICapacityStep[] GetSteps(T data, CastCapacityPhase phase);
    }
}