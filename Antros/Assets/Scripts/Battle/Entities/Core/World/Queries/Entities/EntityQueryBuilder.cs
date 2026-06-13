using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Queries
{
    public delegate bool EntityQueryDelegateFilter(EntityAddress address);

    public struct EntityQueryBuilder
    {
        private readonly struct EntityFilterContainer : IEntityFilter
        {
            private readonly EntityQueryDelegateFilter lambda;

            public EntityFilterContainer(EntityQueryDelegateFilter lambda)
            {
                this.lambda = lambda;
            }

            public bool Accepts(EntityAddress address)
            {
                return lambda(address);
            }
        }

        private EntityQuery entityQuery;


        public EntityQueryBuilder Where(EntityQueryDelegateFilter delegateFilter)
        {
            return Where(new EntityFilterContainer(delegateFilter));
        }
        
        //TODO use static generic to avoid filter boxing
        public EntityQueryBuilder Where<TFilter>(TFilter filter)
            where TFilter : IEntityFilter
        {
            entityQuery.AddFilter(filter);
            return this;
        }

        public EntityQueryBuilder WithAnyComponent<T>() where T : struct, IEntityComponent
        {
            entityQuery.any.Set(ComponentID<T>.ID);
            return this;
        }

        public EntityQueryBuilder WithoutComponent<T>() where T : struct, IEntityComponent
        {
            entityQuery.none.Set(ComponentID<T>.ID);
            return this;
        }

        public EntityQueryBuilder WithAllComponents<T>() where T : struct, IEntityComponent
        {
            entityQuery.all.Set(ComponentID<T>.ID);
            return this;
        }

        public static implicit operator EntityQuery(EntityQueryBuilder builder)
        {
            return builder.entityQuery;
        }
    }
}