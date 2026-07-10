using ATCG.Capacities.Data.Status;
using UnityEngine;

namespace ATCG.Battle.Entities.Runtime.Status
{
    public readonly struct RuntimeStatusContext
    {
        public readonly StatusData statusData;
        public readonly Entity entity;
        public readonly IRuntimeEntity runtimeEntity;
        public readonly Renderer[] renderers;

        public RuntimeStatusContext(StatusData statusData, Entity entity, IRuntimeEntity runtimeEntity)
        {
            this.statusData = statusData;
            this.entity = entity;
            this.runtimeEntity = runtimeEntity;
            renderers = runtimeEntity.Models;
        }
    }
}