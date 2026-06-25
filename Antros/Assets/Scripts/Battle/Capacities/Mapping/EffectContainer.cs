using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Effects
{
    /// <summary>
    /// Concrete container. The struct behaviour lives in the base concrete field;
    /// TryApply calls Apply non-virtually on it (constrained call, no box).
    /// </summary>
    public sealed class EffectContainer<TData, TBehaviour> : Container<TData, TBehaviour>, IEffectContainer
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
}