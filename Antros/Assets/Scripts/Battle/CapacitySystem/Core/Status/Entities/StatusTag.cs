using ATCG.Battle.CapacitySystem.Core.Status.Commands;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public readonly struct StatusTag : IEntityComponent
    {
        public readonly Entity targetEntity;
        public readonly int statusComponentID;
        public readonly int controllerComponentID;
        public readonly StatusData data;

        public StatusTag(StatusData data, int statusComponentID, int controllerComponentID, Entity targetEntity)
        {
            this.data = data;
            this.statusComponentID = statusComponentID;
            this.controllerComponentID = controllerComponentID;
            this.targetEntity = targetEntity;
        }
    }
}