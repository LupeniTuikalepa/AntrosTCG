using ATCG.Capacities.Data.Status;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public readonly struct RuntimeStatusContext
    {
        public readonly StatusData statusData;
        public readonly Entity entity;
        private readonly IRuntimeEntity runtimeEntity;

        public RuntimeStatusContext(StatusData statusData, Entity entity, IRuntimeEntity runtimeEntity)
        {
            this.statusData = statusData;
            this.entity = entity;
            this.runtimeEntity = runtimeEntity;
        }
    }
}