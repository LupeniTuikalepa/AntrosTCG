using System;
using System.Collections.Generic;

namespace Helteix.Tools.DataMapping
{
    /// <summary>
    /// Base mapper = storage + O(1) dispatch only. It does NOT build containers
    /// (a virtual factory here could not carry the domain's precise behaviour
    /// constraint without diluting it to an interface and boxing the struct).
    /// Domains add their own strongly-typed Add that builds the concrete
    /// container directly, then call Register.
    /// </summary>
    public abstract class Mapper<TData, TContainer>
        where TData : IData
        where TContainer : class, IContainer<TData>
    {
        private readonly Dictionary<Type, TContainer> byType = new();

        public void Clear() => byType.Clear();

        /// <summary>Store a fully-built container. One per concrete data type.</summary>
        protected void Register(TContainer container)
        {
            Type key = container.DataType;
            if (!byType.TryAdd(key, container))
            {
                throw new InvalidOperationException(
                    $"A container is already registered for data type {key}. " +
                    $"This mapper allows exactly one behaviour per data type.");
            }
        }

        /// <summary>O(1) dispatch on the data's runtime type (data is a reference).</summary>
        public bool TryGetContainer(TData data, out TContainer container)
        {
            if (data is null)
            {
                container = null;
                return false;
            }

            return byType.TryGetValue(data.GetType(), out container);
        }
    }
}