using System;
using System.Collections.Generic;
using ATCG.Capacities.Properties;

namespace ATCG.Battle.CapacitySystem.Core.Properties
{
    /// <summary>
    /// A closed property schema: pre-filled from a capacity's declared definitions,
    /// then only those names can be written, and only with a matching type. Stores
    /// (declaredType, value) per name — TryGet checks the requested T against the
    /// declared type before returning. Shared by the game context and the editor
    /// preview context so both enforce the same schema.
    /// </summary>
    public sealed class CapacityPropertyBag
    {
        private readonly struct Slot
        {
            public readonly Type type;
            public readonly object value;
            public Slot(Type type, object value) { this.type = type; this.value = value; }
        }

        private readonly Dictionary<string, Slot> slots = new();

        /// <summary>Declares the schema from a capacity's definitions (values seeded to default/null).</summary>
        public void Declare(IReadOnlyList<ICapacityPropertyDefinition> definitions)
        {
            if (definitions == null)
                return;

            for (int i = 0; i < definitions.Count; i++)
            {
                ICapacityPropertyDefinition def = definitions[i];
                if (def == null || string.IsNullOrEmpty(def.Name) || def.PropertyType == null)
                    continue;

                slots[def.Name] = new Slot(def.PropertyType, null);
            }
        }

        /// <summary>
        /// Declares a slot for a well-known built-in key (caster, player, solver...)
        /// that isn't part of the capacity's authored definitions. Seeds a null/default
        /// value; the context then writes the real one via Set.
        /// </summary>
        public void Allow<T>(string name) => slots[name] = new Slot(typeof(T), default(T));

        public bool TryGet<T>(string name, out T value)
        {
            if (slots.TryGetValue(name, out Slot slot) && slot.type == typeof(T) && slot.value is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Writes a declared property. Throws if the name isn't declared or the type
        /// doesn't match the declaration — the schema is closed by design.
        /// </summary>
        public void Set<T>(string name, T value)
        {
            if (!slots.TryGetValue(name, out Slot slot))
                throw new InvalidOperationException(
                    $"[Capacity] Property '{name}' is not declared; declare it on the CapacityData first.");

            if (slot.type != typeof(T))
                throw new InvalidOperationException(
                    $"[Capacity] Property '{name}' is declared as {slot.type.Name} but written as {typeof(T).Name}.");

            slots[name] = new Slot(slot.type, value);
        }

        public bool IsDeclared(string name) => slots.ContainsKey(name);

        /// <summary>
        /// Boxed write for tooling (the editor tweak panel): validates the value's type
        /// against the declaration. Runtime hot paths use the typed Set instead.
        /// </summary>
        public void SetBoxed(string name, object value)
        {
            if (!slots.TryGetValue(name, out Slot slot))
                throw new InvalidOperationException(
                    $"[Capacity] Property '{name}' is not declared.");

            if (value != null && !slot.type.IsInstanceOfType(value))
                throw new InvalidOperationException(
                    $"[Capacity] Property '{name}' is declared as {slot.type.Name} but written as {value.GetType().Name}.");

            slots[name] = new Slot(slot.type, value);
        }

        public void Clear() => slots.Clear();
    }
}