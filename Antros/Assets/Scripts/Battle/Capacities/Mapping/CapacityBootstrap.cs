using ATCG.Battle.Capacities.Mapping;
using ATCG.Battle.Cards.Capacities.Behaviours.Effects;
using ATCG.Battle.Entities.Components.Implementations;
using ATCG.Capacities.Data.Effects;
using ATCG.Capacities.Data.Status;
using Helteix.Tools.DataMapping;
using UnityEngine;

namespace ATCG.Battle.Cards.Capacities
{
    /// <summary>
    /// Replaces BattleDataMapper. No mapper instances, no TryGetFor facade:
    /// registration is global via Mapper.Register, dispatch via Mapper.TryGet&lt;T&gt;.
    /// </summary>
    public static class CapacityBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Edit-mode safety when domain reload is disabled.
            DomainBucket<IEffectContainer>.Clear();
            DomainBucket<IStatusContainer>.Clear();
            
            // Effects
            Mapper.Register<DamageEffectData, DamageEffect>();
            Mapper.Register<HealEffectData, HealEffect>();
            
            //Status
            Mapper.Register<PoisonStatusData, PoisonStatus>();
        }
    }
}