using System;
using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data;
using Helteix.Tools.DataMapping;


public class CapacityEffectMapper : Mapper<IEffectData, CapacityEffectMapper.IEffectContainer>
{
    public interface IEffectContainer : IContainer<IEffectData>
    {
        void TryApply(IEffectData data, EntityAddress target, in CapacityContext capacityContext);
    }

    private sealed class EffectContainer<TData, TBehaviour>
        : Container<TData, TBehaviour>, IEffectContainer
        where TData : IEffectData
        where TBehaviour : ICapacityEffect<TData>
    {
        public EffectContainer(TBehaviour behaviour) : base(behaviour) { }

        public void TryApply(IEffectData data, EntityAddress target, in CapacityContext capacityContext)
        {
            if (data is TData typed)
                behaviour.Apply(typed, target, in capacityContext);
        }
    }

    /// <summary>
    /// Domain Add carries the PRECISE constraint and names the concrete
    /// TBehaviour, so EffectContainer is closed over the real struct type.
    /// No interface intermediary → the struct is never boxed.
    /// </summary>
    public void Add<TData, TBehaviour>(TBehaviour behaviour)
        where TData : IEffectData
        where TBehaviour : ICapacityEffect<TData>
        => Register(new EffectContainer<TData, TBehaviour>(behaviour));

    public void Add<TData, TBehaviour>()
        where TData : IEffectData
        where TBehaviour : ICapacityEffect<TData>, new()
        => Add<TData, TBehaviour>(new TBehaviour());
}