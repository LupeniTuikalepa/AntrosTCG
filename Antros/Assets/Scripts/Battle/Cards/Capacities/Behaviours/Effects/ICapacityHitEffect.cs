using ATCG.Battle.Cards.Capacities.Patterns;
using ATCG.Capacities.Data;
using ATCG.HexGrids;
using UnityEngine;

namespace ATCG.Battle.Cards.Capacities.Effects
{
    public interface ICapacityHitEffect : ICapacityBehaviour<ICapacityHitEffectData>
    {
        //Awaitable Hit(ICapacityHitEffectData data, HexCoordinates coordinates, Capacity capacity);
    }

    public interface ICapacityHitEffect<in T> : ICapacityHitEffect where T : ICapacityHitEffectData
    {
        bool ICapacityBehaviour<ICapacityHitEffectData>.Accepts(ICapacityHitEffectData data) => data is T;
        /*

        async Awaitable ICapacityHitEffect.Hit(ICapacityHitEffectData data, HexCoordinates coordinates, Capacity  capacity)
        {
            if(data is T t)
                await Hit(t, coordinates, capacity);
        }

        Awaitable Hit(T data, HexCoordinates coordinates, Capacity capacity);
        */
    }
}