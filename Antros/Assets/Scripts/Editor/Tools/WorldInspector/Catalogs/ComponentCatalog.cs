using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Read-only view of the registered components (id -> type), sourced from
    /// ComponentRegistry. Built once and cached; cheap to rebuild if the set grows.
    /// </summary>
    public sealed class ComponentCatalog
    {
        public readonly struct Entry
        {
            public readonly int Id;
            public readonly Type Type;

            public Entry(int id, Type type)
            {
                Id = id;
                Type = type;
            }

            public string Name => Type.Name;
        }

        private readonly List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;

        public void Rebuild()
        {
            entries.Clear();
            for (int id = 0; id < ComponentRegistry.MaxComponents; id++)
            {
                Type type = ComponentRegistry.GetTypeForComponentID(id);
                if (type == null)
                    continue;
                entries.Add(new Entry(id, type));
            }
            entries.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        }

        public bool TryGetType(int id, out Type type)
        {
            foreach (Entry e in entries)
            {
                if (e.Id == id)
                {
                    type = e.Type;
                    return true;
                }
            }
            type = null;
            return false;
        }
    }
}