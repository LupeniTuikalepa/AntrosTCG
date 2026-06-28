using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Capacities.Status
{
    [GenerateContainer]
    public interface IStatus<in TData> : IBehaviour<TData> where TData : StatusData
    {
        [AddToContainer]
        void Apply(TData data, EntityAddress target, StatusContext context);

        [AddToContainer]
        void Remove(TData data, EntityAddress address, StatusContext context);

        [AddToContainer]
        void Tick(TData data, EntityAddress address, StatusContext context);

        [AddToContainer]
        void TickAll(TData data, StatusContext context);
    }
}