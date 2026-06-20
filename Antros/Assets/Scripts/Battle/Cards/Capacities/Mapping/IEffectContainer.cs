using ATCG.Battle.Commands.GameCommands;
using ATCG.Battle.Entities;
using ATCG.Capacities.Data;
using Helteix.Tools.DataMapping;

namespace ATCG.Battle.Cards.Capacities.Behaviours.Effects
{
    /// <summary>
    /// Domain container interface (was nested in CapacityEffectMapper). Adds the
    /// business op, never exposes the behaviour — the struct stays unboxed.
    /// </summary>
    public interface IEffectContainer : IContainer<IEffectData>
    {
        void TryApply(IEffectData data, EntityAddress target, in CapacityContext capacityContext);
    }
}