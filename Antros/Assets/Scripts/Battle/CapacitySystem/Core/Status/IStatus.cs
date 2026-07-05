using ATCG.Battle.Entities;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Core.Status
{
    public interface IStatus { }
    [GenerateContainer]
    public interface IStatus<in TData> : IBehaviour<TData>, IStatus where TData : StatusData
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