using System;
using ATCG.Metrics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Enums
{
    [Serializable]
    public struct ElementInfos
    {
        [field: SerializeField, ColorUsage(false)]
        public Color Color { get; private set; }
        [field: SerializeField]
        public string Name { get; private set; }
        [field: SerializeField, PreviewField]
        public Sprite Icon { get; private set; }
    }

    public static class ElementInfosExtensions
    {
        public static ElementInfos GetInfos(this Element element)
        {
            if(GameMetrics.Current.ElementInfos.TryGetValueForKey(element, out ElementInfos value))
                return value;

            return default;
        }
    }
}