using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Queries
{
    public delegate bool EntityQueryFilter(Entity entity, World world);

    public struct EntityQueryBuilder
    {
        private readonly struct EntityQueryFilterContainer : IEntityQueryFilter
        {
            private readonly EntityQueryFilter lambda;

            public EntityQueryFilterContainer(EntityQueryFilter lambda)
            {
                this.lambda = lambda;
            }

            public bool Evaluate(Entity entity, World world)
            {
                return lambda(entity, world);
            }
        }

        private EntityQuery entityQuery;


        public EntityQueryBuilder Where(EntityQueryFilter filter)
        {
            return WithFilter(new EntityQueryFilterContainer(filter));
        }

        public EntityQueryBuilder WithFilter<TFilter>(TFilter filter)
            where TFilter : IEntityQueryFilter
        {
            entityQuery.AddFilter(filter);
            return this;
        }

        public EntityQueryBuilder With<T>() where T : struct, IEntityComponent
        {
            entityQuery.any.Set(ComponentID<T>.ID);
            return this;
        }

        public EntityQueryBuilder Excluding<T>() where T : struct, IEntityComponent
        {
            entityQuery.none.Set(ComponentID<T>.ID);
            return this;
        }

        public EntityQueryBuilder WithAll<T>() where T : struct, IEntityComponent
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