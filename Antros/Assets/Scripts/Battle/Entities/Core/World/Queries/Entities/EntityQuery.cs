using System;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Queries
{
    public struct EntityQuery
    {
        public ComponentMask all;
        public ComponentMask any;
        public ComponentMask none;


        public IEntityQueryFilter[] filters;
        public int filterCount;

        public void AddFilter<TFilter>(TFilter filter)
            where TFilter : IEntityQueryFilter
        {
            filters ??= new IEntityQueryFilter[4];
            if (filterCount >= filters.Length)
                Array.Resize(ref filters, filterCount * 2);

            filters[filterCount++] = filter;
        }

        public readonly bool Matches(in ComponentMask mask)
        {
            return mask.MatchesAll(all)
                   && (any.IsEmpty || mask.MatchesAny(any))
                   && !mask.MatchesAny(none);
        }

        public static EntityQueryBuilder Where(EntityQueryFilter filter)
        {
            return new EntityQueryBuilder().Where(filter);
        }

        public static EntityQueryBuilder WithFilter<TFilter>(TFilter filter) where TFilter : IEntityQueryFilter
        {
            return new EntityQueryBuilder().WithFilter(filter);
        }

        public static EntityQueryBuilder With<T>() where T : struct, IEntityComponent
        {
            return new EntityQueryBuilder().With<T>();
        }

        public static EntityQueryBuilder Excluding<T>() where T : struct, IEntityComponent
        {
            return new EntityQueryBuilder().Excluding<T>();
        }

        public static EntityQueryBuilder WithAll<T>() where T : struct, IEntityComponent
        {
            return new EntityQueryBuilder().WithAll<T>();
        }
    }
}