using ATCG.Capacities;
using Helteix.Tools;
using Helteix.Tools.Phases;
using UnityEngine;

namespace ATCG.Battle.CapacitySystem.Core
{
    public static class CapacityManager
    {
        public static void CastCapacity(CapacityData capacityData, CapacitySetup setup)
        {
            CastCapacityAsync(capacityData, setup).ListenForExceptions();
        }

        public static async Awaitable CastCapacityAsync(CapacityData capacityData, CapacitySetup setup)
        {
            CastCapacityPhase phase = new(
                setup.battlePhase,
                capacityData,
                setup.castPoint,
                setup.caster,
                setup.casterPlayerId);

            await phase.Run();
        }
    }
}