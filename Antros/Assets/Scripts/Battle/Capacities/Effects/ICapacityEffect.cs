using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Effects
{
    /// <summary>Behaviour contract: a data type produces an effect application.</summary>
    ///
    //[MappedBehaviour(typeof(EffectContainer<,>), typeof(IEffectContainer))]


    [GenerateContainer]
    public interface ICapacityEffect<in TData> : IBehaviour<TData> where TData : IEffectData
    {
        [AddToContainer]
        void Apply(TData data, EntityAddress target, in CapacityContext capacityContext);
    }
}