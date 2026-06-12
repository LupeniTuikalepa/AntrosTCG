using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Effects
{
    public interface ICapacityEffect<in T> : IBehaviour<T> where T : IEffectData
    {
        void Apply(T data, EntityAddress target, in CastCapacityCommand.Context context);
    }
}