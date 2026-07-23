using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Runtime;
using ATCG.Battle.Entities.Runtime.VFX;
using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.CapacitySystem.Status.Status
{
    public readonly struct RuntimeStatusContext
    {
        public readonly LinkedRendererGroup renderers => runtimeEntity.Models;

        public readonly StatusData statusData;
        public readonly Entity entity;
        public readonly IRuntimeEntity runtimeEntity;

        public RuntimeStatusContext(StatusData statusData, Entity entity, IRuntimeEntity runtimeEntity)
        {
            this.statusData = statusData;
            this.entity = entity;
            this.runtimeEntity = runtimeEntity;
        }
    }
}