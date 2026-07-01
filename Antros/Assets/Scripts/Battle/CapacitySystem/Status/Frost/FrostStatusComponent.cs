using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Battle.GameModes;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Status.Forst
{
    public struct FrostStatusComponent : IStatusComponent
    {
        private readonly FrostStatusData data;
        StatusData IStatusComponent.StatusData => data;
        
        public FrostStatusComponent(FrostStatusData data)
        {
            this.data = data;
        }
        
        public void Trigger(EntityAddress address, BattlePhase battlePhase)
        {
        }
    }
}