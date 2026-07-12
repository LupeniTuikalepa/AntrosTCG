using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Runtime.VFX
{
    [Serializable]
    public readonly struct LinkedRendererGroup : IEnumerable<Renderer>
    {
        // Indexed per single bit, not per combined Key: a renderer tagged Chest | Clothes
        // is registered under both Chest and Clothes so a query for either one finds it.
        private readonly Dictionary<LinkedRendererKey, HashSet<LinkedRenderer>> renderersByBit;

        [ShowInInspector]
        public List<LinkedRenderer> Renderers => renderersByBit?.Values.SelectMany(ctx => ctx).Distinct().ToList();

        public LinkedRendererGroup(IEnumerable<LinkedRenderer> renderers)
        {
            this.renderersByBit = new();
            foreach (LinkedRenderer renderer in renderers)
            {
                foreach (LinkedRendererKey bit in DecomposeFlags(renderer.Key))
                {
                    if (this.renderersByBit.TryGetValue(bit, out HashSet<LinkedRenderer> set))
                        set.Add(renderer);
                    else
                        this.renderersByBit.Add(bit, new HashSet<LinkedRenderer> { renderer });
                }
            }
        }

        public IEnumerable<LinkedRenderer> GetAll() => renderersByBit.Values.SelectMany(ctx => ctx).Distinct();

        /// <summary>Every renderer that has at least one of the flags in 'mask'.</summary>
        public IEnumerable<LinkedRenderer> GetAllFor(LinkedRendererKey mask)
        {
            using (HashSetPool<LinkedRenderer>.Get(out var s))
            {
                foreach (LinkedRendererKey bit in DecomposeFlags(mask))
                {
                    if (renderersByBit.TryGetValue(bit, out HashSet<LinkedRenderer> set))
                    {
                        foreach (LinkedRenderer renderer in set)
                            s.Add(renderer);
                    }
                }

                foreach (var linkedRenderer in s)
                    yield return linkedRenderer;
            }
        }

        /// <summary>Only renderers that have every flag in 'mask' (exact/precise targeting).</summary>
        public IEnumerable<LinkedRenderer> GetAllForAll(LinkedRendererKey mask)
        {
            foreach (LinkedRenderer renderer in GetAll())
            {
                if (renderer.Key.HasAll(mask))
                    yield return renderer;
            }
        }

        // Splits a combined flags value into its individual set bits. Plain bitwise ops on
        // same-typed enums don't box (unlike Enum.HasFlag), so this stays allocation-light.
        private static IEnumerable<LinkedRendererKey> DecomposeFlags(LinkedRendererKey combined)
        {
            uint bits = (uint)combined;
            for (int i = 0; bits != 0 && i < 32; i++)
            {
                uint bit = 1u << i;
                if ((bits & bit) != 0)
                {
                    yield return (LinkedRendererKey)bit;
                    bits &= ~bit;
                }
            }
        }

        [BurstDiscard]
        public IEnumerator<Renderer> GetEnumerator()
        {
            IEnumerator<Renderer> enumerator = GetAll().Select(ctx => ctx.Renderer).GetEnumerator();
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}