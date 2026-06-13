using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Grids.Patterns;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public interface ICapacityHexPattern<in T, out TPattern> : IBehaviour<T>
        where T : CapacityPatternData
        where TPattern : IHexPattern
    {

        TPattern CreatePattern(T data);
    }
}