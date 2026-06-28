using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Capacities;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    [GenerateContainer]
    public interface ICapacity<in T> : IBehaviour<T> where T : CapacityData
    {

        [AddToContainer]
        IEnumerable<ICapacityStep> Run(T data, CastCapacityPhase phase);
    }
}