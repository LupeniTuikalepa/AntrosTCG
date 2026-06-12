using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Battle.Grids.Patterns.Building;
using ATCG.Capacities.Data;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Patterns
{
    public interface ICapacityHexPattern<in T> : IBehaviour<T> where T : IHexCapacityPatternData
    {
        void AddToBuilder(T data, ref HexPatternBuilder builder);
    }
}