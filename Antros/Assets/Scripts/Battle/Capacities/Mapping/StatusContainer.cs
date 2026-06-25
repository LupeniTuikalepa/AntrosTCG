using ATCG.Battle.Capacities.Status;
using ATCG.Battle.Entities;
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

        public void Apply(StatusData data, EntityAddress address)
        {
            if (data is TStatusData typed)
            {
                behaviour.Apply(typed, address);
            }
        }
    }
}