using System;
using System.Collections.Generic;
using ATCG.Battle.Entities.Components;
using UnityEngine.Pool;

namespace ATCG.Battle.Entities.Lookups
{
    /// <summary>
    /// A query over all components of type <typeparamref name="T"/> with no filter.
    /// </summary>
    public readonly struct ComponentQuery<T> where T : struct, IEntityComponent
    {
        public struct NoFilter : IFilter<T>
        {
            public bool IsValid(in ComponentRef<T> componentRef) => true;
        }

        private readonly ComponentQuery<T, NoFilter> componentQuery;

        public ComponentQuery(ComponentStore<T> store, World world)
        {
            componentQuery = new ComponentQuery<T, NoFilter>(store, new NoFilter(), world);
        }

        public ComponentQuery<T, NoFilter>.Enumerator GetEnumerator() => componentQuery.GetEnumerator();

        public bool Any() => componentQuery.Any();
        public int Count() => componentQuery.Count();
        public bool TryGetFirst(out ComponentRef<T> componentRef) => componentQuery.TryGetFirst(out componentRef);
        public bool TryGetSingle(out ComponentRef<T> componentRef) => componentQuery.TryGetSingle(out componentRef);
    }

    // SYNC: Any operations added here must also be added to EntityQueryResult.
    // Both types intentionally share the same query operation API but cannot
    // share code due to ref struct Enumerator constraints.
    public readonly struct ComponentQuery<T, TFilter>
        where T : struct, IEntityComponent
        where TFilter : IFilter<T>
    {
        private readonly ComponentStore<T> store;
        private readonly TFilter filter;
        private readonly World world;

        public ComponentQuery(ComponentStore<T> store, TFilter filter, World world)
        {
            this.store = store;
            this.filter = filter;
            this.world = world;
        }

        public Enumerator GetEnumerator() => new Enumerator(store, filter, world);

        /// <summary>Returns true if at least one component matches the filter.</summary>
        public bool Any()
        {
            foreach (var _ in this)
                return true;
            return false;
        }

        /// <summary>Returns the number of components matching the filter.</summary>
        public int Count()
        {
            int count = 0;
            foreach (var _ in this)
                count++;
            return count;
        }
        public void FillList(List<ComponentRef<T>> list)
        {
            foreach (var component in this)
                list.Add(component);
        }

        public IDisposable ToList(out List<ComponentRef<T>> list)
        {
            var pool = ListPool<ComponentRef<T>>.Get(out list);
            FillList(list);

            return pool;
        }

        /// <summary>Returns the first matching component, or false if none.</summary>
        public bool TryGetFirst(out ComponentRef<T> componentRef)
        {
            foreach (var item in this)
            {
                componentRef = item;
                return true;
            }
            componentRef = default;
            return false;
        }

        /// <summary>
        /// Returns the single matching component.
        /// Returns false if there are none or more than one match.
        /// </summary>
        public bool TryGetSingle(out ComponentRef<T> componentRef)
        {
            componentRef = default;
            bool found = false;

            foreach (var item in this)
            {
                if (found)
                {
                    componentRef = default;
                    return false;
                }
                componentRef = item;
                found = true;
            }

            return found;
        }

        public ref struct Enumerator
        {
            private int index;
            private readonly World world;
            private readonly ComponentStore<T> store;
            private readonly TFilter filter;

            public Enumerator(ComponentStore<T> store, TFilter filter, World world)
            {
                this.store = store;
                this.filter = filter;
                this.world = world;
                index = -1;
            }

            public bool MoveNext()
            {
                if (store == null)
                    return false;

                while (++index < store.Count)
                {
                    int entityID = store.IndexToEntityID(index);
                    var componentRef = new ComponentRef<T>(world, store, entityID);
                    if (filter.IsValid(in componentRef))
                        return true;
                }

                return false;
            }

            public ComponentRef<T> Current
            {
                get
                {
                    int entityID = store.IndexToEntityID(index);
                    return new ComponentRef<T>(world, store, entityID);
                }
            }
        }
    }
}