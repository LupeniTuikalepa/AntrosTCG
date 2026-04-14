using System;
using ATCG.Battle.Entities.Queries;

namespace ATCG.Battle.Entities
{
    public partial class World
    {
        public EntityQueryResult Query(in Query query)
        {
            return new EntityQueryResult(this, query);
        }

        public void Query(in Query query, Action<Entity> action)
        {
            ReadOnlySpan<int> allElements = entities.AllIDs;
            ReadOnlySpan<EntityMeta> metas = entities.AllElements;

            for (int i = 0; i < entities.Count; i++)
                if (metas[i].MatchesQuery(query))
                    action(new Entity(allElements[i]));
        }

        public void Query<TCallback>(in Query query, ref TCallback callback)
            where TCallback : struct, IQueryCallback
        {
            ReadOnlySpan<int> allElements = entities.AllIDs;
            ReadOnlySpan<EntityMeta> metas = entities.AllElements;

            for (int i = 0; i < entities.Count; i++)
                if (metas[i].MatchesQuery(query))
                    callback.Execute(new Entity(allElements[i]));
        }
    }
}