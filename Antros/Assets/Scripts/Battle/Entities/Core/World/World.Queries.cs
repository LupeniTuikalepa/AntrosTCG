using System;
using ATCG.Battle.Entities.Components;
using ATCG.Battle.Entities.Queries;

namespace ATCG.Battle.Entities
{
    public partial class World
    {
        public EntityQueryResult Query(in EntityQuery entityQuery)
        {
            return new EntityQueryResult(this, entityQuery);
        }

        public void Query(in EntityQuery entityQuery, Action<Entity> action)
        {
            ReadOnlySpan<int> allElements = entities.AllIDs;
            ReadOnlySpan<EntityMeta> metas = entities.AllElements;

            for (int i = 0; i < entities.Count; i++)
                if (metas[i].MatchesQuery(entityQuery))
                    action(new Entity(allElements[i]));
        }

        public void Query<TCallback>(in EntityQuery entityQuery, ref TCallback callback)
            where TCallback : struct, IEntityQueryCallback
        {
            ReadOnlySpan<int> allElements = entities.AllIDs;
            ReadOnlySpan<EntityMeta> metas = entities.AllElements;

            for (int i = 0; i < entities.Count; i++)
                if (metas[i].MatchesQuery(entityQuery))
                    callback.Execute(new Entity(allElements[i]));
        }
    }
}