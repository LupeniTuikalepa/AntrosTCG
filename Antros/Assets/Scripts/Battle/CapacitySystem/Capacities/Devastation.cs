using System.Collections.Generic;
using ATCG.Battle.CapacitySystem.Core;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Capacities;
using ATCG.Capacities.Fire;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Capacities
{
    public partial struct Devastation : ICapacity<DevastationData>
    {
        public IEnumerable<ICapacityStep> Run(DevastationData data, CastCapacityPhase phase)
        {
            throw new System.NotImplementedException();
        }
    }

}