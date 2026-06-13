using System;
using ATCG.Battle.Entities.Components;

namespace ATCG.Battle.Entities.Queries
{
    public struct EntityQuery
    {
        public ComponentMask all;
        public ComponentMask any;
        public ComponentMask none;


        public IEntityFilter[] filters;
        public int filterCount;

        public void AddFilter<TFilter>(TFilter filter)
            where TFilter : IEntityFilter
        {
            filters ??= new IEntityFilter[4];
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

        public static EntityQueryBuilder Where(EntityQueryDelegateFilter delegateFilter)
        {
            return new EntityQueryBuilder().Where(delegateFilter);
        }

        public static EntityQueryBuilder WithFilter<TFilter>(TFilter filter) where TFilter : IEntityFilter
        {
            return new EntityQueryBuilder().Where(filter);
        }

        public static EntityQueryBuilder With<T>() where T : struct, IEntityComponent
        {
            return new EntityQueryBuilder().WithAnyComponent<T>();
        }

        public static EntityQueryBuilder Excluding<T>() where T : struct, IEntityComponent
        {
            return new EntityQueryBuilder().WithoutComponent<T>();
        }

        public static EntityQueryBuilder WithAll<T>() where T : struct, IEntityComponent
        {
            return new EntityQueryBuilder().WithAllComponents<T>();
        }
    }
}