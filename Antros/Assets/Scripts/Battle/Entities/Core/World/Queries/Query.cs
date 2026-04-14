using System;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Queries
{
    public struct Query
    {
        public ComponentMask all;
        public ComponentMask any;
        public ComponentMask none;


        public IQueryFilter[] filters;
        public int filterCount;

        public void AddFilter<TFilter>(TFilter filter)
            where TFilter : IQueryFilter
        {
            filters ??= new IQueryFilter[4];
            filterCount++;
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

        public static QueryBuilder Where(QueryFilter filter)
        {
            return new QueryBuilder().Where(filter);
        }

        public static QueryBuilder WithFilter<TFilter>(TFilter filter) where TFilter : IQueryFilter
        {
            return new QueryBuilder().WithFilter(filter);
        }

        public static QueryBuilder With<T>() where T : struct, IEntityComponent
        {
            return new QueryBuilder().With<T>();
        }

        public static QueryBuilder Excluding<T>() where T : struct, IEntityComponent
        {
            return new QueryBuilder().Excluding<T>();
        }

        public static QueryBuilder WithAll<T>() where T : struct, IEntityComponent
        {
            return new QueryBuilder().WithAll<T>();
        }
    }
}