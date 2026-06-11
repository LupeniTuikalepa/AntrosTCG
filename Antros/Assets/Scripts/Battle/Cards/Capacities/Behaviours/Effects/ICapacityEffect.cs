using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Effects
{
    public interface ICapacityEffect
    {
        void TryApplyEffectTo(IEffectData data, EntityAddress target, in CastCapacityCommand.Context context);
    }

    public interface ICapacityEffect<in T> : ICapacityEffect where T : IEffectData
    {
        
        void ICapacityEffect.TryApplyEffectTo(IEffectData data, EntityAddress target,
            in CastCapacityCommand.Context context)
        {
            if (data is T t)
                Hit(t, target, in context);
        }

        void Hit(T data, EntityAddress target, in CastCapacityCommand.Context context);
    }
}