using System;
using ATCG.Battle.Entities;
using ATCG.Battle.Utilities.Iterations;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.CapacitySystem.Core.Status
{

    [GenerateIterator]
    public interface IStatus { }

    [GenerateContainer]
    public interface IStatus<in TData> : IBehaviour<TData>, IStatus
        where TData : StatusData
    {
        [AddToContainer]
        bool Is(TData data, Type type) => this.GetType() == type;

        [AddToContainer]
        void Apply(TData data, EntityAddress target, StatusContext context);

        [AddToContainer]
        void Remove(TData data, EntityAddress target, StatusContext context);

        [AddToContainer]
        void Tick(TData data, EntityAddress target, StatusContext context);
    }
}