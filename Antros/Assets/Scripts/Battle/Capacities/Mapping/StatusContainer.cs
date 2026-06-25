using ATCG.Battle.Capacities.Status;
using ATCG.Battle.Entities;
using ATCG.Battle.Entities.Components.Status;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Capacities.Mapping
{
    public sealed class StatusContainer<TStatusData, TStatus> :  Container<TStatusData, TStatus>, IStatusContainer 
        where TStatusData : StatusData
        where TStatus : IStatus<TStatusData>
    {
        public StatusContainer(TStatus behaviour) : base(behaviour)
        {
        }

        public void Apply(StatusData data, EntityAddress address, StatusContext context)
        {
            if (data is TStatusData typed)
            {
                behaviour.Apply(typed, address, context);
            }
        }

        public void Remove(StatusData data, EntityAddress address, StatusContext context)
        {
            if (data is TStatusData typed)
            {
                behaviour.Remove(typed, address, context);
            }
        }

        public void Tick(StatusData data, EntityAddress address, StatusContext context)
        {
            if (data is TStatusData typed)
            {
                behaviour.Tick(typed, address, context);
            }
        }

        public void TickAll(StatusData data, StatusContext context)
        {
            if (data is TStatusData typed)
            {
                behaviour.TickAll(typed, context);
            }
        }
    }
}