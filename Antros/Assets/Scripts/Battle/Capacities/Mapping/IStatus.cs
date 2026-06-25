using ATCG.Battle.Capacities.Mapping;
using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Capacities.Status
{
    [MappedBehaviour(typeof(StatusContainer<,>), typeof(IStatusContainer))]
    public interface IStatus<in TData> : IBehaviour<TData> where TData : StatusData
    {
        void Apply(TData data, EntityAddress target);
    }
}