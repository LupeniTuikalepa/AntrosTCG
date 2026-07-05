using ATCG.Battle.CapacitySystem.Core.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.GameModes;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Status.Forst
{
    public readonly struct FreezeStatusComponent : IStatusComponent
    {
        private readonly FreezeStatusData data;
        StatusData IStatusComponent.StatusData => data;

        public FreezeStatusComponent(FreezeStatusData data)
        {
            this.data = data;
        }

        public void Trigger(EntityAddress address, BattlePhase battlePhase)
        {
        }
    }
}