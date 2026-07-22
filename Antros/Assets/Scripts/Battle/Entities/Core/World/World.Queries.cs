using System;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Lookups;
using ATCG.Battle.Entities.Queries;

namespace ATCG.Battle.Entities
{
    public partial class World
    {
        public EntityQueryResult Query(in EntityQuery entityQuery)
        {
            return new EntityQueryResult(this, entityQuery);
        }

        public ComponentQuery<T, TFilter> Query<TFilter, T>(in TFilter filter)
            where TFilter : IFilter<T>
            where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (stores[id] is not ComponentStore<T> store)
                return default;

            return new ComponentQuery<T, TFilter>(store, filter, this);
        }

        public ComponentQuery<T> Query<T>() where T : struct, IEntityComponent
        {
            int id = ComponentID<T>.ID;
            if (stores[id] is not ComponentStore<T> store)
                return default;

            return new ComponentQuery<T>(store, this);
        }

        /// <summary>
        /// Gets all entities that match the lambda
        /// </summary>
        /// <param name="entityQuery"></param>
        /// <param name="action"></param>
        public void Query(in EntityQuery entityQuery, Action<Entity> action)
        {
            ReadOnlySpan<int> allElements = entities.AllIDs;
            ReadOnlySpan<EntityMeta> metas = entities.AllElements;

            for (int i = 0; i < entities.Count; i++)
                if (metas[i].MatchesQuery(entityQuery))
                    action(new Entity(allElements[i], GetGeneration(allElements[i])));
        }

        public void Query<TCallback>(in EntityQuery entityQuery, ref TCallback callback)
            where TCallback : struct, IEntityQueryCallback
        {
            ReadOnlySpan<int> allElements = entities.AllIDs;
            ReadOnlySpan<EntityMeta> metas = entities.AllElements;

            for (int i = 0; i < entities.Count; i++)
                if (metas[i].MatchesQuery(entityQuery))
                    callback.Execute(new Entity(allElements[i], GetGeneration(allElements[i])));
        }
    }
}