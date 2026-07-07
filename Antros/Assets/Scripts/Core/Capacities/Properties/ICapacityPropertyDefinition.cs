using System;
using UnityEngine;

namespace ATCG.Capacities.Properties
{
    /// <summary>
    /// A declared capacity property: a name and a type. Pure schema — no value. Values
    /// are injected at runtime (baked data) or supplied as editor debug values for
    /// preview, both stored outside the asset. Each concrete definition covers one
    /// element type and can represent either a single value or an array via IsArray;
    /// PropertyType reflects that (T or T[]).
    /// </summary>
    public interface ICapacityPropertyDefinition
    {
        string Name { get; set; }
        bool IsArray { get; set; }
        Type PropertyType { get; }
        Type ElementType { get; }
    }

    /// <summary>
    /// Base for capacity property definitions. Holds the shared name + isArray flag and
    /// derives PropertyType from the subclass ElementType (T, or T[] when IsArray).
    /// One subclass per element type; array vs single is a flag, not a separate class.
    /// </summary>
    [Serializable]
    public abstract class CapacityPropertyDefinition : ICapacityPropertyDefinition
    {
        [SerializeField]
        private string name;
        [SerializeField]
        private bool isArray;

        public string Name { get => name; set => name = value; }
        public bool IsArray { get => isArray; set => isArray = value; }

        public abstract Type ElementType { get; }

        public Type PropertyType => isArray ? ElementType.MakeArrayType() : ElementType;
    }
}