using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Effects
{
    public interface ICapacityEffect : ICapacityBehaviour<IEffectData>
    {
        void Hit(IEffectData data, EntityAddress target, in CastCapacityCommand.Context context);
    }

    public interface ICapacityEffect<in T> : ICapacityEffect where T : IEffectData
    {
        bool ICapacityBehaviour<IEffectData>.Accepts(IEffectData data)
        {
            return data is T;
        }

        void ICapacityEffect.Hit(IEffectData data, EntityAddress target,
            in CastCapacityCommand.Context context)
        {
            if (data is T t)
                Hit(t, target, in context);
        }

        void Hit(T data, EntityAddress target, in CastCapacityCommand.Context context);
    }
}