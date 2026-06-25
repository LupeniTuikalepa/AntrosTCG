using System;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Capacities.Mapping
{
    public interface IStatusContainer : IContainer<StatusData>
    {
        public void Apply(StatusData data, EntityAddress address);
        
    }
}