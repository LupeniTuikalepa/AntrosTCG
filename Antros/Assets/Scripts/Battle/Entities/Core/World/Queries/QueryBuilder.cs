using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Queries
{
    public delegate bool QueryFilter(Entity entity, World world);

    public struct QueryBuilder
    {
        private readonly struct QueryFilterContainer : IQueryFilter
        {
            private readonly QueryFilter lambda;

            public QueryFilterContainer(QueryFilter lambda)
            {
                this.lambda = lambda;
            }

            public bool Evaluate(Entity entity, World world)
            {
                return lambda(entity, world);
            }
        }

        private Query query;


        public QueryBuilder Where(QueryFilter filter)
        {
            return WithFilter(new QueryFilterContainer(filter));
        }

        public QueryBuilder WithFilter<TFilter>(TFilter filter)
            where TFilter : IQueryFilter
        {
            query.AddFilter(filter);
            return this;
        }

        public QueryBuilder With<T>() where T : struct, IEntityComponent
        {
            query.any.Set(ComponentID<T>.ID);
            return this;
        }

        public QueryBuilder Excluding<T>() where T : struct, IEntityComponent
        {
            query.none.Set(ComponentID<T>.ID);
            return this;
        }

        public QueryBuilder WithAll<T>() where T : struct, IEntityComponent
        {
            query.all.Set(ComponentID<T>.ID);
            return this;
        }

        public static implicit operator Query(QueryBuilder builder)
        {
            return builder.query;
        }
    }
}