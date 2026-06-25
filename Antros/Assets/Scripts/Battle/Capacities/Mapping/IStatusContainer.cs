using System;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Capacities.Mapping
{
    public interface IStatusContainer : IContainer<StatusData>
    {
        public void Apply(StatusData data, EntityAddress address, StatusContext context);
        public void Remove(StatusData data, EntityAddress address, StatusContext context);
        public void Tick(StatusData data, EntityAddress address, StatusContext context);

        public void TickAll(StatusData data, StatusContext context);

    }
}