using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Lookups
{
    /// <summary>
    ///     Lookups are an easy way to search for specific components with certain conditions.
    ///     Where Queries enable you to go through all entities, Lookups are more design to go through all components that
    ///     meets certain requirements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ComponentsLookupResult<T> where T : struct, IEntityComponent
    {
        public readonly struct NoComponentsLookupFilter : IComponentLookupFilter<T>
        {
            public bool IsValid(in ComponentRef<T> componentRef)
            {
                return true;
            }
        }

        private readonly ComponentsLookupResult<T, NoComponentsLookupFilter> lookupResult;

        public ComponentsLookupResult(ComponentStore<T> store, World world)
        {
            lookupResult =
                new ComponentsLookupResult<T, NoComponentsLookupFilter>(store, new NoComponentsLookupFilter(), world);
        }

        public ComponentsLookupResult<T, NoComponentsLookupFilter>.Enumerator GetEnumerator()
        {
            return lookupResult.GetEnumerator();
        }
    }

    /// <summary>
    ///     Lookups are an easy way to search for specific components with certain conditions.
    ///     Where Queries enable you to go through all entities, Lookups are more design to go through all components that
    ///     meets certain requirements.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TFilter"></typeparam>
    public readonly struct ComponentsLookupResult<T, TFilter>
        where T : struct, IEntityComponent
        where TFilter : IComponentLookupFilter<T>
    {
        private readonly ComponentStore<T> store;
        private readonly TFilter filter;
        private readonly World world;

        public Enumerator GetEnumerator()
        {
            return new Enumerator(store, filter, world);
        }

        public ComponentsLookupResult(ComponentStore<T> store, TFilter filter, World world)
        {
            this.store = store;
            this.filter = filter;
            this.world = world;
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