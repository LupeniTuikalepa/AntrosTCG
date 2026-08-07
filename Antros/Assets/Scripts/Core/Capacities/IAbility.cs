using ATCG.Elements;
using ATCG.Enums;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ATCG.Capacities
{
    public interface IAbility
    {
        public string Name { get; }

        public Element Element { get; }

        public string Description { get; }
    }
}