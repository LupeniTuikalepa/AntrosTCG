using System;
using System.Collections.Generic;
using System.Reflection;

namespace ATCG.Battle.Entities.EditorTools
{
    /// <summary>
    /// Discovers entity aspects and exposes a matcher per aspect.
    ///
    /// Aspects don't carry a readable ComponentMask; instead the source generator emits
    /// a static `bool IsAspect(EntityAddress)` on each aspect (a plain AND of HasComponent
    /// checks). We bind to that method by reflection and invoke it per entity. This is the
    /// exact runtime matching logic, with no aspect instantiation and no mask rebuild.
    /// </summary>
    public sealed class AspectCatalog
    {
        public sealed class Entry
        {
            public readonly Type Type;
            private readonly MethodInfo isAspect;

            public Entry(Type type, MethodInfo isAspect)
            {
                Type = type;
                this.isAspect = isAspect;
            }

            public string Name => Type.Name;

            public bool Matches(in EntityAddress address)
            {
                try
                {
                    object result = isAspect.Invoke(null, new object[] { address });
                    return result is true;
                }
                catch (System.Exception e)
                {
                    InspectorLog.Warn($"Aspect {Name}.IsAspect threw during matching", e);
                    return false;
                }
            }
        }

        private readonly List<Entry> entries = new();

        public IReadOnlyList<Entry> Entries => entries;

        public void Rebuild()
        {
            entries.Clear();

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                // Some dynamic/foreign assemblies throw on GetTypes; skipping them is normal.
                try { types = asm.GetTypes(); }
                catch { continue; }

                foreach (Type type in types)
                {
                    if (!IsConcreteAspect(type))
                        continue;

                    MethodInfo isAspect = FindIsAspect(type);
                    if (isAspect == null)
                        continue;

                    entries.Add(new Entry(type, isAspect));
                }
            }

            entries.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        }

        private static bool IsConcreteAspect(Type type)
        {
            if (type.IsAbstract || type.IsInterface)
                return false;
            if (!typeof(IEntityAspect).IsAssignableFrom(type))
                return false;
            return type.IsValueType;
        }

        /// <summary>
        /// The generated `static bool IsAspect(EntityAddress)` is the matcher. We accept
        /// any static method named IsAspect taking a single EntityAddress and returning bool.
        /// </summary>
        private static MethodInfo FindIsAspect(Type type)
        {
            foreach (MethodInfo m in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (m.Name != "IsAspect" || m.ReturnType != typeof(bool))
                    continue;
                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType == typeof(EntityAddress))
                    return m;
            }
            return null;
        }
    }
}
